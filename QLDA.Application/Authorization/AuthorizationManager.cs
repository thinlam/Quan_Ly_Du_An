namespace QLDA.Application.Authorization;

/// <summary>
/// Scoped façade that routes authorization calls to the correct provider.
/// All IAuthorizationProvider instances are injected via DI; the manager builds
/// a resourceKey → provider map once at construction time.
/// IAuthorizationContext is also injected so provider + context share the same
/// scope (and therefore the same DbContext, IUserProvider, etc.).
/// </summary>
public class AuthorizationManager(
    IAuthorizationContext context,
    IEnumerable<IAuthorizationProvider> providers) : IAuthorizationManager
{
    private readonly IAuthorizationContext _context = context;
    private readonly Dictionary<string, IAuthorizationProvider> _providers = BuildProviderMap(providers);

    private static Dictionary<string, IAuthorizationProvider> BuildProviderMap(
        IEnumerable<IAuthorizationProvider> providers)
    {
        var map = new Dictionary<string, IAuthorizationProvider>();
        foreach (var provider in providers)
        {
            foreach (var key in AuthorizationResourceKeys.All)
            {
                if (!provider.CanHandle(key)) continue;

                if (!map.TryAdd(key, provider))
                    throw new InvalidOperationException(
                        $"Multiple IAuthorizationProvider instances handle resource '{key}'. " +
                        $"Registered: {map[key].GetType().Name}, conflicting: {provider.GetType().Name}.");
            }
        }
        return map;
    }

    public IAuthorizationContext Context => _context;

    public async Task<bool> CanExecuteAsync(string resourceKey, object entity, CancellationToken ct)
    {
        var provider = ResolveProvider(resourceKey);
        return await provider.CanExecuteAsync(entity, _context, ct);
    }

    public async Task<bool> CanViewAsync(string resourceKey, object entity, CancellationToken ct)
    {
        var provider = ResolveProvider(resourceKey);
        return await provider.CanViewAsync(entity, _context, ct);
    }

    public IQueryable<T> FilterVisible<T>(IQueryable<T> query, string resourceKey) where T : class
    {
        var provider = ResolveProvider(resourceKey);
        return provider.Filter(query, _context);
    }

    private IAuthorizationProvider ResolveProvider(string resourceKey)
    {
        if (!_providers.TryGetValue(resourceKey, out var provider))
            throw new InvalidOperationException(
                $"No authorization provider registered for resource '{resourceKey}'. " +
                $"Available: [{string.Join(", ", _providers.Keys)}]");

        return provider;
    }
}
