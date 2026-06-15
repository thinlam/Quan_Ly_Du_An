namespace QLDA.Application.Authorization;

/// <summary>
/// Base contract for authorization providers.
/// Each provider handles specific resource keys (e.g., "DuAn", "DuAnBuoc").
/// Providers are registered via IAuthorizationManager.
/// </summary>
public interface IAuthorizationProvider
{
    /// <summary>
    /// Returns true if this provider handles the given resource key.
    /// </summary>
    bool CanHandle(string resourceKey);

    /// <summary>
    /// Check if user can execute (write) on the entity.
    /// </summary>
    Task<bool> CanExecuteAsync(object entity, IAuthorizationContext ctx, CancellationToken ct);

    /// <summary>
    /// Check if user can view (read) the entity.
    /// </summary>
    Task<bool> CanViewAsync(object entity, IAuthorizationContext ctx, CancellationToken ct);

    /// <summary>
    /// Apply visibility filter to IQueryable for read operations.
    /// </summary>
    IQueryable<T> Filter<T>(IQueryable<T> query, IAuthorizationContext ctx) where T : class;
}
