using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLDA.Domain.Entities;

namespace QLDA.Application.Providers;

/// <summary>
/// Policy provider - reads role-permission toggles from CauHinhVaiTroQuyen table
/// Caches mappings in memory, refreshes on each request scope
/// </summary>
public class PolicyProvider : IPolicyProvider {
    private readonly IServiceProvider _serviceProvider;
    private Dictionary<string, HashSet<string>>? _cache;

    public PolicyProvider(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public bool HasPermission(IEnumerable<string> roles, string permissionKey) {
        var mapping = GetOrLoadMapping();
        foreach (var role in roles) {
            if (mapping.TryGetValue(role, out var permissions) && permissions.Contains(permissionKey))
                return true;
        }
        return false;
    }

    public List<string> GetActivePermissions(IEnumerable<string> roles) {
        var mapping = GetOrLoadMapping();
        var result = new HashSet<string>();
        foreach (var role in roles) {
            if (mapping.TryGetValue(role, out var permissions))
                result.UnionWith(permissions);
        }
        return [.. result];
    }

    public bool CanViewAll(IUserProvider user, string xemTatCaPermission) {
        return HasPermission(user.AuthInfo.Roles, xemTatCaPermission);
    }

    public bool CanViewByPhong(IUserProvider user, string xemTheoPhongPermission) {
        return HasPermission(user.AuthInfo.Roles, xemTheoPhongPermission);
    }

    /// <summary>
    /// Load role-permission mapping from DB (lazy, cached per instance)
    /// </summary>
    private Dictionary<string, HashSet<string>> GetOrLoadMapping() {
        if (_cache != null) return _cache;

        var repo = _serviceProvider.GetRequiredService<IRepository<CauHinhVaiTroQuyen, int>>();
        var mappings = repo.GetQueryableSet(OnlyUsed: false, OnlyNotDeleted: false)
            .Include(c => c.Quyen)
            .Where(c => c.KichHoat && !c.IsDeleted)
            .Where(c => c.Quyen != null)
            .Select(c => new { c.VaiTro, Ma = c.Quyen!.Ma })
            .ToList();

        _cache = [];
        foreach (var m in mappings) {
            if (!_cache.TryGetValue(m.VaiTro, out var set)) {
                set = [];
                _cache[m.VaiTro] = set;
            }
            if (!string.IsNullOrEmpty(m.Ma))
                set.Add(m.Ma);
        }

        return _cache;
    }
}
