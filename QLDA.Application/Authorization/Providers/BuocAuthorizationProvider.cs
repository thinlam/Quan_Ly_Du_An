using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Providers;
using QLDA.Domain.Enums;
using PermissionConstants = QLDA.Domain.Constants.PermissionConstants;

namespace QLDA.Application.Authorization;

public class BuocAuthorizationProvider(
    IAppSettingsProvider settings,
    IPolicyProvider policy) : IBuocAuthorizationProvider
{
    public bool HasGlobalBypass(IUserProvider user)
    {
        if (user.Info.PhongBanID == settings.PhongKHTCID)
            return true;

        if (policy.CanViewAll(user, PermissionConstants.DuAn_XemTatCa))
            return true;

        return false;
    }

    public Task<bool> CanExecuteStepAsync(DuAnBuoc buoc, IUserProvider user, CancellationToken ct)
    {
        if (HasGlobalBypass(user))
            return Task.FromResult(true);

        var userId = user.Info.UserID;
        if (userId > 0 && buoc.DuAn?.LanhDaoPhuTrachId == userId)
            return Task.FromResult(true);

        var phongBanId = user.Info.PhongBanID ?? 0;

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

    public IQueryable<DuAnBuoc> FilterVisibleSteps(IQueryable<DuAnBuoc> query, IUserProvider user)
    {
        if (HasGlobalBypass(user))
            return query;

        var phongBanId = user.Info.PhongBanID ?? 0;
        var userId = user.Info.UserID;
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
        IRepository<DuAnBuoc, int> buocRepo,
        IUserProvider user,
        Func<T, int?> buocIdSelector) where T : class
    {
        if (HasGlobalBypass(user))
            return query;

        var phongBanId = user.Info.PhongBanID ?? 0;
        var userId = user.Info.UserID;
        if (phongBanId == 0 && userId <= 0)
            return query.Where(e => false);

        var visibleBuocIds = buocRepo.GetQueryableSet()
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
}
