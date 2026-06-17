using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;

namespace QLDA.Application.Authorization;

/// <summary>
/// Authorization helper cho Bước (DuAnBuoc).
/// Chuẩn hóa cách check ownership cho Bước - dùng chung cho cả CanExecute, Filter, Child entity filter.
/// </summary>
public static class BuocAuthorizationHelper
{
    /// <summary>
    /// Tạo filter expression cho ownership check của Bước.
    /// Logic:
    /// 1. User là Lãnh Đạo Phụ Trách DuAn → được
    /// 2. Phòng ban chính của Bước = Phòng ban của user → được
    /// 3. User nằm trong danh sách PBPH của Bước → được
    /// 4. Bước chưa gán PB nào VÀ user thuộc scope DuAn → được
    /// </summary>
    public static Expression<Func<DuAnBuoc, bool>> BuildOwnershipFilter(long userId, long? phongBanId)
    {
        var phongBanIdValue = phongBanId ?? 0;
        var param = Expression.Parameter(typeof(DuAnBuoc), "b");

        // Điều kiện 1: User là Lãnh Đạo Phụ Trách DuAn
        var isLanhDao = BuildLanhDaoCondition(param, userId);

        // Điều kiện 2: Phòng ban phụ trách chính
        var isPhongBanChinh = BuildPhongBanChinhCondition(param, phongBanIdValue);

        // Điều kiện 3: Trong danh sách phối hợp
        var isPhongBanPhoiHop = BuildPhongBanPhoiHopCondition(param, phongBanIdValue);

        // Điều kiện 4: Fallback - Bước chưa gán PB nhưng user thuộc scope DuAn
        var isFallbackScope = BuildFallbackDuAnScopeCondition(param, phongBanIdValue);

        // Combine: IsLanhDao || IsPhongBanChinh || IsPhongBanPhoiHop || IsFallbackScope
        var combinedBody = Expression.OrElse(
            Expression.OrElse(isLanhDao, isPhongBanChinh),
            Expression.OrElse(isPhongBanPhoiHop, isFallbackScope));

        return Expression.Lambda<Func<DuAnBuoc, bool>>(combinedBody, param);
    }

    /// <summary>
    /// Tạo subquery expression để lấy visible BuocIds.
    /// </summary>
    public static Expression<Func<DuAnBuoc, bool>> BuildVisibleBuocIdsFilter(
        IQueryable<DuAnBuoc> baseQuery,
        long userId,
        long? phongBanId)
    {
        if (phongBanId == 0 && userId <= 0)
            return _ => false;

        return BuildOwnershipFilter(userId, phongBanId);
    }

    private static BinaryExpression BuildLanhDaoCondition(ParameterExpression param, long userId)
    {
        // b.DuAn != null && b.DuAn.LanhDaoPhuTrachId == userId
        var duAnProperty = Expression.Property(param, "DuAn");
        var lanhDaoProperty = Expression.Property(duAnProperty, "LanhDaoPhuTrachId");
        var nullCheck = Expression.NotEqual(duAnProperty, Expression.Constant(null, typeof(DuAn)));
        var compareCheck = Expression.Equal(lanhDaoProperty, Expression.Constant(userId));
        return Expression.AndAlso(nullCheck, compareCheck);
    }

    private static BinaryExpression BuildPhongBanChinhCondition(ParameterExpression param, long phongBanId)
    {
        // b.PhongPhuTrachChinhId == phongBanIdValue
        var property = Expression.Property(param, "PhongPhuTrachChinhId");
        return Expression.Equal(property, Expression.Constant(phongBanId));
    }

    private static BinaryExpression BuildPhongBanPhoiHopCondition(ParameterExpression param, long phongBanId)
    {
        // b.DuAnBuocPhongBanPhoiHops != null && b.DuAnBuocPhongBanPhoiHops.Any(p => p.RightId == phongBanIdValue)
        var phongBanPhoiHops = Expression.Property(param, "DuAnBuocPhongBanPhoiHops");
        var nullCheck = Expression.NotEqual(phongBanPhoiHops, Expression.Constant(null, typeof(ICollection<DuAnBuocPhongBanPhoiHop>)));
        var anyCall = BuildAnyCall(phongBanPhoiHops, phongBanId);
        return Expression.AndAlso(nullCheck, anyCall);
    }

    private static BinaryExpression BuildFallbackDuAnScopeCondition(ParameterExpression param, long phongBanId)
    {
        // b.PhongPhuTrachChinhId == null
        // && (b.DuAnBuocPhongBanPhoiHops == null || !b.DuAnBuocPhongBanPhoiHops.Any())
        // && b.DuAn != null
        // && (b.DuAn.DonViPhuTrachChinhId == phongBanIdValue
        //     || b.DuAn.DuAnChiuTrachNhiemXuLys!.Any(x => x.RightId == phongBanIdValue))

        var phongPhuTrachChinh = Expression.Property(param, "PhongPhuTrachChinhId");
        var nullPhongChinh = Expression.Equal(phongPhuTrachChinh, Expression.Constant(null, typeof(long?)));

        var phongBanPhoiHops = Expression.Property(param, "DuAnBuocPhongBanPhoiHops");
        var nullOrEmptyPhoiHop = Expression.OrElse(
            Expression.Equal(phongBanPhoiHops, Expression.Constant(null, typeof(ICollection<DuAnBuocPhongBanPhoiHop>))),
            Expression.Not(BuildAnyCall(phongBanPhoiHops, phongBanId)));

        var duAn = Expression.Property(param, "DuAn");
        var notNullDuAn = Expression.NotEqual(duAn, Expression.Constant(null, typeof(DuAn)));

        // b.DuAn.DonViPhuTrachChinhId == phongBanIdValue
        var donViPhuTrachChinh = Expression.Property(duAn, "DonViPhuTrachChinhId");
        var matchDonVi = Expression.Equal(donViPhuTrachChinh, Expression.Constant(phongBanId));

        // b.DuAn.DuAnChiuTrachNhiemXuLys!.Any(x => x.RightId == phongBanIdValue)
        var chiuTrachNhiem = Expression.Property(duAn, "DuAnChiuTrachNhiemXuLys");
        var anyChiuTrachNhiem = BuildAnyChiuTrachNhiemCall(chiuTrachNhiem, phongBanId);

        var inScopeDuAn = Expression.OrElse(matchDonVi, anyChiuTrachNhiem);

        return Expression.AndAlso(
            Expression.AndAlso(nullPhongChinh, nullOrEmptyPhoiHop),
            Expression.AndAlso(notNullDuAn, inScopeDuAn));
    }

    private static MethodCallExpression BuildAnyCall(Expression collection, long phongBanId)
    {
        // collection.Any(p => p.RightId == phongBanIdValue)
        var anyMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(DuAnBuocPhongBanPhoiHop));

        var param = Expression.Parameter(typeof(DuAnBuocPhongBanPhoiHop), "p");
        var rightIdProperty = Expression.Property(param, "RightId");
        var condition = Expression.Equal(rightIdProperty, Expression.Constant(phongBanId));
        var lambda = Expression.Lambda(condition, param);

        return Expression.Call(anyMethod, collection, lambda);
    }

    private static MethodCallExpression BuildAnyChiuTrachNhiemCall(Expression collection, long phongBanId)
    {
        // collection.Any(x => x.RightId == phongBanIdValue)
        var anyMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(DuAnChiuTrachNhiemXuLy));

        var param = Expression.Parameter(typeof(DuAnChiuTrachNhiemXuLy), "x");
        var rightIdProperty = Expression.Property(param, "RightId");
        var condition = Expression.Equal(rightIdProperty, Expression.Constant(phongBanId));
        var lambda = Expression.Lambda(condition, param);

        return Expression.Call(anyMethod, collection, lambda);
    }

    /// <summary>
    /// Kiểm tra ownership trên object đã load (không phải IQueryable).
    /// </summary>
    public static bool CheckOwnership(DuAnBuoc buoc, long userId, long? phongBanId)
    {
        var filter = BuildOwnershipFilter(userId, phongBanId);
        return filter.Compile()(buoc);
    }
}

/// <summary>
/// Authorization cho step (DuAnBuoc):
/// - CanExecuteStep (write): HasKhtcBypass OR ownership
/// - FilterVisibleSteps: filter qua ownership scope
/// - FilterVisibleChildEntities: filter child entity qua subquery visible buoc ids
///
/// Authorization flags (HasKhtcBypass) are computed once per request
/// by AuthorizationContext and exposed via IAuthorizationContext parameter.
/// </summary>
public class BuocAuthorizationProvider(IRepository<DuAnBuoc, int> buocRepo) : IBuocAuthorizationProvider
{
    public async Task<bool> CanExecuteStepAsync(DuAnBuoc buoc, IAuthorizationContext ctx, CancellationToken ct)
    {
        if (ctx.HasKhtcBypass) return true;

        return BuocAuthorizationHelper.CheckOwnership(buoc, ctx.UserId, ctx.PhongBanId);
    }

    public IQueryable<DuAnBuoc> FilterVisibleSteps(IQueryable<DuAnBuoc> query, IAuthorizationContext ctx)
    {
        if (ctx.HasKhtcBypass) return query;

        if (ctx.PhongBanId == 0 && ctx.UserId <= 0)
            return query.Where(e => false);

        var filter = BuocAuthorizationHelper.BuildOwnershipFilter(ctx.UserId, ctx.PhongBanId);
        return query.Where(filter);
    }

    public IQueryable<T> FilterVisibleChildEntities<T>(
        IQueryable<T> query,
        IRepository<DuAnBuoc, int> buocRepository,
        IAuthorizationContext ctx,
        Func<T, int?> buocIdSelector) where T : class
    {
        if (ctx.HasKhtcBypass) return query;

        if (ctx.PhongBanId == 0 && ctx.UserId <= 0)
            return query.Where(e => false);

        var visibleBuocIds = buocRepository.GetQueryableSet()
            .Where(BuocAuthorizationHelper.BuildOwnershipFilter(ctx.UserId, ctx.PhongBanId))
            .Select(b => b.Id);

        return ApplyChildBuocIdFilter(query, visibleBuocIds, buocIdSelector);
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

    private static IQueryable<T> ApplyChildBuocIdFilter<T>(
        IQueryable<T> query,
        IQueryable<int> visibleBuocIds,
        Func<T, int?> buocIdSelector) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var buocIdProperty = Expression.Invoke(
            Expression.Constant(buocIdSelector),
            parameter);
        var containsMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(int));

        // e => visibleBuocIds.Contains(buocIdSelector(e))
        var containsCall = Expression.Call(
            containsMethod,
            visibleBuocIds.Expression,
            buocIdProperty);

        // e => buocIdSelector(e) == null || visibleBuocIds.Contains(buocIdSelector(e))
        var nullCheck = Expression.Equal(buocIdProperty, Expression.Constant(null, typeof(int?)));
        var combinedCondition = Expression.OrElse(nullCheck, containsCall);

        var lambda = Expression.Lambda<Func<T, bool>>(combinedCondition, parameter);
        return query.Where(lambda);
    }
}
