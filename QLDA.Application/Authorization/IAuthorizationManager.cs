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

    /// <summary>
    /// Kiểm tra quyền duyệt / trả lại / từ chối phê duyệt trên một DuAn cụ thể.
    /// Cho phép khi user thuộc role QLDA_LDDV HOẶC là Lãnh đạo phụ trách chính của DuAn
    /// (DuAn.LanhDaoPhuTrachId == userId).
    /// Throw ForbiddenException nếu cả hai điều kiện đều sai.
    /// Noop khi duAnId là Guid.Empty (entity không gắn với DuAn nào).
    /// </summary>
    Task EnsureCanApproveDuAnAsync(Guid duAnId, CancellationToken ct = default);

    /// <summary>
    /// Check quyền Insert/Update/Delete trên VanBanQuyetDinh-derived entity.
    /// - Nếu buocId có giá trị: dùng BuocAuth.EnsureCanExecuteStepAsync (Owner/Lãnh đạo)
    /// - Nếu buocId null: fallback check DuAn ownership qua DuAnAuthorizationProvider.EnsureCanExecuteAsync
    /// User có role QLDA_QuanTri hoặc HasKhtcBypass bypass hoàn toàn.
    /// </summary>
    Task EnsureCanExecuteAsync(int? buocId, Guid duAnId, IAuthorizationContext ctx, CancellationToken ct = default);
}
