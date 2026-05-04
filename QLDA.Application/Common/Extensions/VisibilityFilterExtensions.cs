using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;
using QLDA.Application.Providers;
using PermissionConstants = QLDA.Domain.Constants.PermissionConstants;

namespace QLDA.Application.Common.Extensions;

/// <summary>
/// Visibility filter extensions for IQueryable based on user's role-permission toggles
/// </summary>
public static class VisibilityFilterExtensions {
    /// <summary>
    /// Apply DuAn visibility filter: if user has XemTatCa → show all; if XemTheoPhong → show own department's projects only
    /// </summary>
    public static IQueryable<DuAn> ApplyDuAnVisibility(this IQueryable<DuAn> query, IUserProvider user, IPolicyProvider policy) {
        if (policy.CanViewAll(user, PermissionConstants.DuAn_XemTatCa))
            return query;

        if (policy.CanViewByPhong(user, PermissionConstants.DuAn_XemTheoPhong) && user.Info.PhongBanID.HasValue) {
            var phongBanId = user.Info.PhongBanID.Value;
            return query.Where(e =>
                e.DonViPhuTrachChinhId == phongBanId ||
                e.DuAnChiuTrachNhiemXuLys!.Any(i => i.RightId == phongBanId));
        }

        // No permission → no data
        return query.Where(e => false);
    }

    /// <summary>
    /// Apply visibility for entities that belong to a DuAn (GoiThau, HopDong, VanBan, etc.)
    /// Filters by user's DuAn visibility via subquery
    /// </summary>
    public static IQueryable<T> ApplyDuAnChildVisibility<T>(
        this IQueryable<T> query,
        IRepository<DuAn, Guid> duAnRepo,
        IUserProvider user,
        IPolicyProvider policy,
        Func<T, Guid> duAnIdSelector) where T : class {
        if (policy.CanViewAll(user, PermissionConstants.DuAn_XemTatCa))
            return query;

        if (policy.CanViewByPhong(user, PermissionConstants.DuAn_XemTheoPhong) && user.Info.PhongBanID.HasValue) {
            var phongBanId = user.Info.PhongBanID.Value;
            var visibleDuAnIds = duAnRepo.GetQueryableSet()
                .Where(e =>
                    e.DonViPhuTrachChinhId == phongBanId ||
                    e.DuAnChiuTrachNhiemXuLys!.Any(i => i.RightId == phongBanId))
                .Select(e => e.Id);

            return query.Where(e => visibleDuAnIds.Contains(duAnIdSelector(e)));
        }

        return query.Where(e => false);
    }
}
