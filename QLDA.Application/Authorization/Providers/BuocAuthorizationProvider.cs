using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.Authorization;

/// <summary>
/// Authorization cho step (DuAnBuoc):
/// - CanExecuteStep (write): HasKhtcBypass OR ownership (admin/manager KHÔNG còn bypass ownership)
/// - FilterVisibleSteps: filter qua ownership scope
/// - FilterVisibleChildEntities: filter child entity (KeHoachTrienKhai...) qua subquery visible buoc ids
///
/// Authorization flags (HasKhtcBypass, IsAdminManager) are computed once per request
/// by AuthorizationContext and exposed via IAuthorizationContext parameter.
/// </summary>
public class BuocAuthorizationProvider(IRepository<DuAnBuoc, int> buocRepo) : IBuocAuthorizationProvider
{
    public Task<bool> CanExecuteStepAsync(DuAnBuoc buoc, IAuthorizationContext ctx, CancellationToken ct)
    {
        if (ctx.HasKhtcBypass) return Task.FromResult(true);

        var userId = ctx.UserId;
        if (userId > 0 && buoc.DuAn?.LanhDaoPhuTrachId == userId)
            return Task.FromResult(true);

        var phongBanId = ctx.PhongBanId ?? 0;

        if (buoc.PhongPhuTrachChinhId == phongBanId)
            return Task.FromResult(true);

        if (buoc.DuAnBuocPhongBanPhoiHops?.Any(p => p.RightId == phongBanId) == true)
            return Task.FromResult(true);

        if (buoc.PhongPhuTrachChinhId == null
            && (buoc.DuAnBuocPhongBanPhoiHops?.Count ?? 0) == 0
            && buoc.DuAn != null)
        {
            var inDuAnScope =
                buoc.DuAn.DonViPhuTrachChinhId == phongBanId
                || buoc.DuAn.DuAnChiuTrachNhiemXuLys?
                    .Any(x => x.RightId == phongBanId
                          && x.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop) == true;
            if (inDuAnScope)
                return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public IQueryable<DuAnBuoc> FilterVisibleSteps(IQueryable<DuAnBuoc> query, IAuthorizationContext ctx)
    {
        if (ctx.HasKhtcBypass) return query;

        var phongBanId = ctx.PhongBanId ?? 0;
        var userId = ctx.UserId;
        if (phongBanId == 0 && userId <= 0)
            return query.Where(e => false);

        return query.Where(e =>
            (e.DuAn != null && e.DuAn.LanhDaoPhuTrachId == userId)
            || e.PhongPhuTrachChinhId == phongBanId
            || (e.DuAnBuocPhongBanPhoiHops != null
                && e.DuAnBuocPhongBanPhoiHops.Any(p => p.RightId == phongBanId))
            || (e.PhongPhuTrachChinhId == null
                && (e.DuAnBuocPhongBanPhoiHops == null || !e.DuAnBuocPhongBanPhoiHops.Any())
                && e.DuAn != null
                && (e.DuAn.DonViPhuTrachChinhId == phongBanId
                    || e.DuAn.DuAnChiuTrachNhiemXuLys!
                        .Any(x => x.RightId == phongBanId
                              && x.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop))));
    }

    public IQueryable<T> FilterVisibleChildEntities<T>(
        IQueryable<T> query,
        IRepository<DuAnBuoc, int> buocRepository,
        IAuthorizationContext ctx,
        Func<T, int?> buocIdSelector) where T : class
    {
        if (ctx.HasKhtcBypass) return query;

        var phongBanId = ctx.PhongBanId ?? 0;
        var userId = ctx.UserId;
        if (phongBanId == 0 && userId <= 0)
            return query.Where(e => false);

        var visibleBuocIds = buocRepository.GetQueryableSet()
            .Where(e =>
                (e.DuAn != null && e.DuAn.LanhDaoPhuTrachId == userId)
                || e.PhongPhuTrachChinhId == phongBanId
                || (e.DuAnBuocPhongBanPhoiHops != null
                    && e.DuAnBuocPhongBanPhoiHops.Any(p => p.RightId == phongBanId))
                || (e.PhongPhuTrachChinhId == null
                    && (e.DuAnBuocPhongBanPhoiHops == null || !e.DuAnBuocPhongBanPhoiHops.Any())
                    && e.DuAn != null
                    && (e.DuAn.DonViPhuTrachChinhId == phongBanId
                        || e.DuAn.DuAnChiuTrachNhiemXuLys!
                            .Any(x => x.RightId == phongBanId
                                  && x.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop))))
            .Select(b => b.Id);

        return query.Where(e =>
            buocIdSelector(e) == null
            || visibleBuocIds.Contains(buocIdSelector(e)!.Value));
    }

    public async Task EnsureCanExecuteStepAsync(int? buocId, IAuthorizationContext ctx, CancellationToken ct = default)
    {
        if (!buocId.HasValue) return;

        var buoc = await buocRepo.GetQueryableSet()
            .Include(e => e.DuAn)
            .Include(e => e.DuAnBuocPhongBanPhoiHops)
            .FirstOrDefaultAsync(e => e.Id == buocId.Value, ct);

        if (buoc != null && !await CanExecuteStepAsync(buoc, ctx, ct))
            throw new ManagedException("Phòng ban không có quyền thao tác bước này");
    }
}
