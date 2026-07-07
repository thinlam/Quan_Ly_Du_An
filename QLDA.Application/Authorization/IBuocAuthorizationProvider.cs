using System.Linq.Expressions;
using QLDA.Domain.Entities;

namespace QLDA.Application.Authorization;

public interface IBuocAuthorizationProvider
{
    Task<bool> CanExecuteStepAsync(DuAnBuoc buoc, IAuthorizationContext ctx, CancellationToken ct);
    IQueryable<DuAnBuoc> FilterVisibleSteps(IQueryable<DuAnBuoc> query, IAuthorizationContext ctx);
    IQueryable<T> FilterVisibleChildEntities<T>(
        IQueryable<T> query,
        IRepository<DuAnBuoc, int> buocRepo,
        IAuthorizationContext ctx,
        Expression<Func<T, int?>> buocIdSelector) where T : class;

    /// <summary>
    /// Kiểm tra quyền thao tác bước dự án. Throw ManagedException nếu user không có quyền.
    /// Noop khi buocId null.
    /// </summary>
    Task EnsureCanExecuteStepAsync(int? buocId, IAuthorizationContext ctx, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra quyền chỉnh sửa danh sách phòng ban phối hợp (DanhSachPhongBanPhoiHopIds).
    /// Chỉ Owner (CreatedBy) + Lãnh đạo phụ trách mới có quyền.
    /// </summary>
    Task<bool> CanManageViewerListAsync(DuAnBuoc buoc, IAuthorizationContext ctx, CancellationToken ct);

    /// <summary>
    /// Throw ManagedException nếu user không có quyền chỉnh DanhSachPhongBanPhoiHopIds.
    /// </summary>
    Task EnsureCanManageViewerListAsync(int buocId, IAuthorizationContext ctx, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra quyền edit/delete các field của bước (TenBuoc, Ngay, ManHinh, PhongPhuTrachChinhId).
    /// Chỉ Owner (CreatedBy) + Lãnh đạo phụ trách mới có quyền.
    /// </summary>
    Task<bool> CanManageStepFieldsAsync(DuAnBuoc buoc, IAuthorizationContext ctx, CancellationToken ct);

    /// <summary>
    /// Throw ManagedException nếu user không có quyền edit/delete các field của bước.
    /// Noop khi buocId null.
    /// </summary>
    Task EnsureCanManageStepFieldsAsync(int? buocId, IAuthorizationContext ctx, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra quyền Insert/Update ThanhToan: Owner + Lãnh đạo + role thuộc GroupAdminCatalog + PhongBanChinh.
    /// PhongBanPhoiHop KHÔNG có quyền (kể cả khi thuộc DuAn.ChiuTrachNhiemXuLys).
    /// </summary>
    Task<bool> CanExecuteThanhToanAsync(DuAnBuoc buoc, IAuthorizationContext ctx, CancellationToken ct);

    /// <summary>
    /// Throw ManagedException nếu user không có quyền Insert/Update ThanhToan.
    /// </summary>
    Task EnsureCanExecuteThanhToanAsync(int? buocId, IAuthorizationContext ctx, CancellationToken ct = default);
}
