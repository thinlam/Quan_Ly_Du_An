using System.Linq.Expressions;
using System.Reflection;
using BuildingBlocks.CrossCutting.Exceptions;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.Authorization;

/// <summary>
/// Authorization cho resource "DuAn":
/// - CanExecute (write): HasAdminCatalog OR ownership. HasReadAllBypass KHÔNG tự bypass
///                       write — user có role read-all vẫn phải match ownership
///                       (Lãnh đạo phụ trách / Phòng phụ trách chính / Phối hợp) mới CUD được.
///                       Cho phép NVTT_XemDuAn CUD DuAn được assign.
/// - CanView (read): HasAdminCatalog OR HasReadAllBypass OR ownership
/// - Filter&lt;T&gt;:
///     * T = DuAn: apply ownership filter on DuAn rows (skipped khi HasAdminCatalog/HasReadAllBypass)
///     * T có property DuAnId (HopDong, GoiThau, VanBan...): subquery filter on visible DuAn ids
///     * T khác: fail-safe empty result
///
/// Ownership logic:
/// 1. LanhDaoPhuTrachId == userId → được (ALL - không cần check phòng)
/// 2. CreatedBy == userId → được (chỉ bản ghi mình tạo)
/// 3. DonViPhuTrachChinhId == phongBanId → được (ALL trong phòng)
/// 4. DuAnChiuTrachNhiemXuLys.Any(RightId == phongBanId) → được (ALL trong phòng)
///
/// Authorization flags (HasAdminCatalog, HasReadAllBypass) are computed once
/// per request by AuthorizationContext and exposed via IAuthorizationContext parameter.
/// </summary>
public class DuAnAuthorizationProvider(IRepository<DuAn, Guid> duAnRepo) : IAuthorizationProvider
{
    private const string DuAnIdPropertyName = "DuAnId";

    public bool CanHandle(string resourceKey) => resourceKey == AuthorizationResourceKeys.DuAn;

    public async Task<bool> CanExecuteAsync(object entity, IAuthorizationContext ctx, CancellationToken ct)
    {
        if (ctx.HasAdminCatalog) return true;
        // HasReadAllBypass: không tự bypass write — ownership check phía sau quyết định.
        // NVTT_XemDuAn user khi assign DuAn sẽ match ownership → CUD được.
        if (entity is not DuAn duAn) return false;
        return await CheckOwnershipAsync(ctx, duAn.Id, ct);
    }

    public async Task<bool> CanViewAsync(object entity, IAuthorizationContext ctx, CancellationToken ct)
    {
        if (ctx.HasAdminCatalog) return true;
        if (ctx.HasReadAllBypass) return true;
        if (entity is not DuAn duAn) return false;
        return await CheckOwnershipAsync(ctx, duAn.Id, ct);
    }

    public IQueryable<T> Filter<T>(IQueryable<T> query, IAuthorizationContext ctx) where T : class
    {
        if (ctx.HasAdminCatalog) return query;
        if (ctx.HasReadAllBypass) return query;

        if (query is IQueryable<DuAn> daQuery)
            return (IQueryable<T>)ApplyDuAnOwnershipFilter(daQuery, ctx);

        // Child entity (HopDong, GoiThau, VanBanPhapLy, VanBanChuTruong, ...) — filter qua subquery DuAn
        var duAnIdProperty = typeof(T).GetProperty(DuAnIdPropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (duAnIdProperty is null || duAnIdProperty.PropertyType != typeof(Guid))
            return query.Where(e => false);

        var visibleDuAnIds = BuildVisibleDuAnIdsQuery(ctx);
        return ApplyChildDuAnIdFilter(query, duAnIdProperty, visibleDuAnIds);
    }

    /// <summary>
    /// Throw ForbiddenException nếu user không có quyền write DuAn. Noop khi duAnId null.
    /// </summary>
    public async Task EnsureCanExecuteAsync(Guid? duAnId, IAuthorizationContext ctx, CancellationToken ct = default)
    {
        if (!duAnId.HasValue) return;

        var duAn = await duAnRepo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == duAnId.Value, ct);
        if (duAn is null) return;

        if (!await CanExecuteAsync(duAn, ctx, ct))
            throw new ForbiddenException("User không có quyền thao tác dự án này");
    }

    private IQueryable<DuAn> ApplyDuAnOwnershipFilter(IQueryable<DuAn> daQuery, IAuthorizationContext ctx)
    {
        var phongBanId = ctx.PhongBanId ?? 0;
        var userId = ctx.UserId;
        if (phongBanId == 0 && userId <= 0)
            return daQuery.Where(e => false);

        return daQuery.Where(e =>
            e.LanhDaoPhuTrachId == userId ||
            e.CreatedBy == userId.ToString() ||
            e.DonViPhuTrachChinhId == phongBanId ||
            e.DuAnChiuTrachNhiemXuLys!.Any(x =>
                x.RightId == phongBanId && x.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop));
    }

    private IQueryable<Guid> BuildVisibleDuAnIdsQuery(IAuthorizationContext ctx)
    {
        var phongBanId = ctx.PhongBanId ?? 0;
        var userId = ctx.UserId;
        if (phongBanId == 0 && userId <= 0)
            return duAnRepo.GetQueryableSet().Where(e => false).Select(e => e.Id);

        return duAnRepo.GetQueryableSet()
            .Where(e =>
                e.LanhDaoPhuTrachId == userId ||
                e.CreatedBy == userId.ToString() ||
                e.DonViPhuTrachChinhId == phongBanId ||
                e.DuAnChiuTrachNhiemXuLys!.Any(x =>
                    x.RightId == phongBanId && x.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop))
            .Select(e => e.Id);
    }

    private IQueryable<T> ApplyChildDuAnIdFilter<T>(
        IQueryable<T> query,
        PropertyInfo duAnIdProperty,
        IQueryable<Guid> visibleDuAnIds) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var propertyAccess = Expression.Property(parameter, duAnIdProperty);
        var containsMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(Guid));
        var containsCall = Expression.Call(
            containsMethod,
            visibleDuAnIds.Expression,
            propertyAccess);
        var lambda = Expression.Lambda<Func<T, bool>>(containsCall, parameter);

        return query.Where(lambda);
    }

    private async Task<bool> CheckOwnershipAsync(IAuthorizationContext ctx, Guid duAnId, CancellationToken ct)
    {
        var phongBanId = ctx.PhongBanId ?? 0;
        var userId = ctx.UserId;
        if (phongBanId == 0 && userId <= 0) return false;

        return await duAnRepo.GetQueryableSet()
            .Where(d => d.Id == duAnId)
            .AnyAsync(d =>
                d.LanhDaoPhuTrachId == userId ||
                d.CreatedBy == userId.ToString() ||
                d.DonViPhuTrachChinhId == phongBanId ||
                d.DuAnChiuTrachNhiemXuLys!.Any(x =>
                    x.RightId == phongBanId && x.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop),
                ct);
    }
}
