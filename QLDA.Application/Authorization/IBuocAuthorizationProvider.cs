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
        Func<T, int?> buocIdSelector) where T : class;

    /// <summary>
    /// Kiểm tra quyền thao tác bước dự án. Throw ManagedException nếu user không có quyền.
    /// Noop khi buocId null.
    /// </summary>
    Task EnsureCanExecuteStepAsync(int? buocId, IAuthorizationContext ctx, CancellationToken ct = default);
}
