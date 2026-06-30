namespace QLDA.Application.Authorization;

/// <summary>
/// Scoped context per HTTP request.
/// Holds current user info + cached authorization flags, computed once per request.
/// Providers inject this instead of IUserProvider/IAppSettingsProvider/IPolicyProvider directly.
/// </summary>
public interface IAuthorizationContext
{
    /// <summary>
    /// Current user provider.
    /// </summary>
    IUserProvider User { get; }

    /// <summary>
    /// Current user's ID.
    /// </summary>
    long UserId { get; }

    /// <summary>
    /// Current user's department ID (PhongBanID from JWT).
    /// </summary>
    long? PhongBanId { get; }

    /// <summary>
    /// True if user belongs to PhongKHTC department — bypasses all ownership checks.
    /// Cached, computed once per request.
    /// </summary>
    bool HasKhtcBypass { get; }

    /// <summary>
    /// True if user holds an admin/manager role (DuAn-level: RoleConstants.GroupAdminOrManager)
    /// OR has DuAn_XemTatCa policy (Buoc-level).
    /// Cached, computed once per request.
    /// </summary>
    bool IsAdminManager { get; }

    /// <summary>
    /// Combined bypass: HasKhtcBypass || IsAdminManager.
    /// Provided for places that historically treated admin/manager as bypass.
    /// Note: most providers should use the specific flag they need, not this aggregate.
    /// </summary>
    bool HasGlobalBypass { get; }

    /// <summary>
    /// True if user holds any role in <see cref="QLDA.Domain.Constants.RoleConstants.GroupReadAll"/>
    /// — grants read-only access to all DuAn, Bước, and child entities (HopDong, GoiThau,
    /// VanBan...) through DuAnId-based filtering. Does NOT grant write access.
    /// Cached, computed once per request.
    /// </summary>
    bool HasReadAllBypass { get; }

    /// <summary>
    /// True khi user thuộc <see cref="QLDA.Domain.Constants.RoleConstants.GroupAdminCatalog"/>
    /// (QLDA_TatCa, QLDA_QuanTri) HOẶC thuộc Phòng KHTC. Bypass ownership check trên
    /// DuAn/Buoc (cả read và write). Tương đương HasKhtcBypass về mặt bypass filter,
    /// nhưng dựa trên role chứ không chỉ dựa trên phòng ban.
    /// KHÁC <see cref="HasGlobalBypass"/> ở chỗ KHÔNG bao gồm QLDA_LDDV — Lãnh đạo
    /// đơn vị vẫn phải qua ownership filter (chỉ bypass khi là Lãnh đạo phụ trách DuAn).
    /// Cached, computed once per request.
    /// </summary>
    bool HasAdminCatalog { get; }

    /// <summary>
    /// Get LanhDaoPhuTrachId for a DuAn, cached per-DuAn per request.
    /// </summary>
    Task<long?> GetLanhDaoPhuTrachIdAsync(Guid duAnId, CancellationToken ct);
}
