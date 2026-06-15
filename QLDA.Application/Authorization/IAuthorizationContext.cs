namespace QLDA.Application.Authorization;

/// <summary>
/// Scoped context per HTTP request.
/// Holds current user info + cached bypass flags.
/// </summary>
public interface IAuthorizationContext
{
    /// <summary>
    /// Current user provider.
    /// </summary>
    IUserProvider User { get; }

    /// <summary>
    /// True if user has global bypass (KHTC identity or admin role).
    /// Cached — computed once per request.
    /// </summary>
    bool HasGlobalBypass { get; }

    /// <summary>
    /// Current user's ID.
    /// </summary>
    long UserId { get; }

    /// <summary>
    /// Current user's department ID (PhongBanID from JWT).
    /// </summary>
    long? PhongBanId { get; }

    /// <summary>
    /// Get LanhDaoPhuTrachId for a DuAn, cached per-DuAn per request.
    /// </summary>
    Task<long?> GetLanhDaoPhuTrachIdAsync(Guid duAnId, CancellationToken ct);
}
