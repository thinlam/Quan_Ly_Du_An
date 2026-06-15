namespace QLDA.Application.Authorization;

/// <summary>
/// Singleton façade for authorization.
/// Resolves the correct provider by resource key and delegates authorization calls.
/// </summary>
public interface IAuthorizationManager
{
    /// <summary>
    /// Scoped context holding current user + cached bypass flags.
    /// </summary>
    IAuthorizationContext Context { get; }

    /// <summary>
    /// Register a provider for a resource key.
    /// Throws InvalidOperationException if resourceKey is already registered (fail-fast).
    /// </summary>
    void RegisterProvider(string resourceKey, IAuthorizationProvider provider);

    /// <summary>
    /// Check if user can execute (write) on the entity.
    /// </summary>
    Task<bool> CanExecuteAsync(string resourceKey, object entity, CancellationToken ct);

    /// <summary>
    /// Check if user can view (read) the entity.
    /// </summary>
    Task<bool> CanViewAsync(string resourceKey, object entity, CancellationToken ct);

    /// <summary>
    /// Apply visibility filter to IQueryable.
    /// </summary>
    IQueryable<T> FilterVisible<T>(IQueryable<T> query, string resourceKey) where T : class;
}
