using System.Collections.Concurrent;
using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Providers;
using QLDA.Domain.Entities;
using PermissionConstants = QLDA.Domain.Constants.PermissionConstants;
using RoleConstants = QLDA.Domain.Constants.RoleConstants;

namespace QLDA.Application.Authorization;

/// <summary>
/// Scoped implementation of IAuthorizationContext.
/// Caches all authorization flags (HasKhtcBypass, IsAdminManager, HasGlobalBypass) and
/// LanhDaoPhuTrachId lookups per request.
/// </summary>
public class AuthorizationContext(
    IUserProvider user,
    IAppSettingsProvider settings,
    IPolicyProvider policy,
    IServiceProvider serviceProvider) : IAuthorizationContext
{
    private readonly IUserProvider _user = user;
    private readonly IAppSettingsProvider _settings = settings;
    private readonly IPolicyProvider _policy = policy;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private bool? _hasKhtcBypass;
    private bool? _isAdminManager;
    private bool? _hasGlobalBypass;
    private bool? _hasReadAllBypass;
    private readonly ConcurrentDictionary<Guid, long?> _lanhDaoCache = new();

    public IUserProvider User => _user;

    public long UserId => _user.Info.UserID;

    public long? PhongBanId => _user.Info.PhongBanID;

    public bool HasKhtcBypass => _hasKhtcBypass ??= ComputeHasKhtcBypass();

    public bool IsAdminManager => _isAdminManager ??= ComputeIsAdminManager();

    public bool HasGlobalBypass => _hasGlobalBypass ??= HasKhtcBypass || IsAdminManager;

    public bool HasReadAllBypass => _hasReadAllBypass ??= ComputeHasReadAllBypass();

    public async Task<long?> GetLanhDaoPhuTrachIdAsync(Guid duAnId, CancellationToken ct)
    {
        if (_lanhDaoCache.TryGetValue(duAnId, out var cached))
            return cached;

        var repo = _serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        var lanhDaoId = await repo.GetQueryableSet()
            .Where(d => d.Id == duAnId)
            .Select(d => d.LanhDaoPhuTrachId)
            .FirstOrDefaultAsync(ct);

        _lanhDaoCache.TryAdd(duAnId, lanhDaoId);
        return lanhDaoId;
    }

    private bool ComputeHasKhtcBypass()
        => _user.Info.PhongBanID == _settings.PhongKHTCID;

    private bool ComputeHasReadAllBypass()
    {
        var userRoles = _user.AuthInfo.Roles ?? [];
        if (userRoles.Count == 0) return false;
        var readAllRoles = RoleConstants.GroupReadAll.Split(',');
        return userRoles.Any(r => readAllRoles.Contains(r?.Trim() ?? "", StringComparer.Ordinal));
    }

    private bool ComputeIsAdminManager()
    {
        // Role check: covers admin/manager roles configured in RoleConstants
        var roles = _user.AuthInfo.Roles ?? [];
        var adminManagerRoles = RoleConstants.GroupAdminOrManager.Split(',');
        foreach (var r in roles)
            if (adminManagerRoles.Contains(r.Trim())) return true;

        // Policy check: covers users with DuAn_XemTatCa permission (used by BuocProvider historically)
        if (_policy.CanViewAll(_user, PermissionConstants.DuAn_XemTatCa))
            return true;

        return false;
    }
}
