using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Providers;

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
    IServiceProvider serviceProvider) : IAuthorizationContext {
    private readonly IUserProvider _user = user;
    private readonly IAppSettingsProvider _settings = settings;
    private readonly IPolicyProvider _policy = policy;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private bool? _hasKhtcBypass;
    private readonly ConcurrentDictionary<Guid, long?> _lanhDaoCache = new();

    public IUserProvider User => _user;

    public long UserId => _user.Info.UserID;

    public long? PhongBanId => _user.Info.PhongBanID;

    public bool HasKhtcBypass => _hasKhtcBypass ??= ComputeHasKhtcBypass();

    public async Task<long?> GetLanhDaoPhuTrachIdAsync(Guid duAnId, CancellationToken ct) {
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
        => _user.Info.PhongBanID == _settings.PhongKHTCId;

}
