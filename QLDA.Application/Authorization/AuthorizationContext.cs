using System.Collections.Concurrent;
using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Providers;
using QLDA.Domain.Entities;
using PermissionConstants = QLDA.Domain.Constants.PermissionConstants;

namespace QLDA.Application.Authorization;

/// <summary>
/// Scoped implementation of IAuthorizationContext.
/// Caches HasGlobalBypass and LanhDaoPhuTrachId per request.
/// </summary>
public class AuthorizationContext : IAuthorizationContext
{
    private readonly IUserProvider _user;
    private readonly IAppSettingsProvider _settings;
    private readonly IPolicyProvider _policy;
    private readonly IServiceProvider _serviceProvider;

    private bool? _hasGlobalBypass;
    private readonly ConcurrentDictionary<Guid, long?> _lanhDaoCache = new();

    public AuthorizationContext(
        IUserProvider user,
        IAppSettingsProvider settings,
        IPolicyProvider policy,
        IServiceProvider serviceProvider)
    {
        _user = user;
        _settings = settings;
        _policy = policy;
        _serviceProvider = serviceProvider;
    }

    public IUserProvider User => _user;

    public long UserId => _user.Info.UserID;

    public long? PhongBanId => _user.Info.PhongBanID;

    public bool HasGlobalBypass => _hasGlobalBypass ??= ComputeHasGlobalBypass();

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

    private bool ComputeHasGlobalBypass()
    {
        if (_user.Info.PhongBanID == _settings.PhongKHTCID)
            return true;

        if (_policy.CanViewAll(_user, PermissionConstants.DuAn_XemTatCa))
            return true;

        return false;
    }
}
