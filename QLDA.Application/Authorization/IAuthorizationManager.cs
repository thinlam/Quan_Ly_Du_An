namespace QLDA.Application.Authorization;

/// <summary>
/// Scoped façade for authorization.
/// Resolves the correct provider by resource key and delegates authorization calls.
/// DI injects all registered IAuthorizationProvider instances; the manager
/// builds a resourceKey → provider map once at construction.
/// </summary>
public interface IAuthorizationManager
{
    /// <summary>
    /// Scoped context holding current user + cached bypass flags.
    /// </summary>
    IAuthorizationContext Context { get; }

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
