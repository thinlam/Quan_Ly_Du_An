using System.Collections.Concurrent;

namespace QLDA.Application.Authorization;

/// <summary>
/// Singleton façade that routes authorization calls to the correct provider.
/// Providers are registered at startup via RegisterProvider().
/// IAuthorizationContext is resolved per-call from IServiceProvider to respect scoped lifetime.
/// </summary>
public class AuthorizationManager : IAuthorizationManager
{
    private readonly ConcurrentDictionary<string, IAuthorizationProvider> _providers = new();
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IAuthorizationContext Context
        => _serviceProvider.GetRequiredService<IAuthorizationContext>();

    public void RegisterProvider(string resourceKey, IAuthorizationProvider provider)
    {
        if (!_providers.TryAdd(resourceKey, provider))
            throw new InvalidOperationException(
                $"Authorization provider for resource '{resourceKey}' is already registered.");
    }

    public async Task<bool> CanExecuteAsync(string resourceKey, object entity, CancellationToken ct)
    {
        var provider = ResolveProvider(resourceKey);
        return await provider.CanExecuteAsync(entity, Context, ct);
    }

    public async Task<bool> CanViewAsync(string resourceKey, object entity, CancellationToken ct)
    {
        var provider = ResolveProvider(resourceKey);
        return await provider.CanViewAsync(entity, Context, ct);
    }

    public IQueryable<T> FilterVisible<T>(IQueryable<T> query, string resourceKey) where T : class
    {
        var provider = ResolveProvider(resourceKey);
        return provider.Filter(query, Context);
    }

    private IAuthorizationProvider ResolveProvider(string resourceKey)
    {
        if (_providers.TryGetValue(resourceKey, out var provider))
            return provider;

        throw new InvalidOperationException(
            $"No authorization provider registered for resource '{resourceKey}'. " +
            $"Available: [{string.Join(", ", _providers.Keys)}]");
    }
}
