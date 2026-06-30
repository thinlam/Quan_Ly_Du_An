using System.Linq.Expressions;
using System.Reflection;
using BuildingBlocks.CrossCutting.Exceptions;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.Authorization;

/// <summary>
/// Authorization helper cho Bước (DuAnBuoc).
/// Chuẩn hóa cách check ownership cho Bước - dùng chung cho cả CanExecute, Filter, Child entity filter.
/// </summary>
public static class BuocAuthorizationHelper
{
    /// <summary>
    /// Tạo filter expression cho ownership check của Bước.
    /// Logic (sau khi siết phân quyền):
    /// 1. User là Lãnh Đạo Phụ Trách DuAn → được (ALL - không cần check phòng)
    /// 2. User là người tạo bước → được (chỉ bản ghi mình tạo)
    /// 3. Phòng ban chính của Bước = Phòng ban của user → được
    /// 4. User nằm trong danh sách PBPH của Bước AND user thuộc DuAn.DuAnChiuTrachNhiemXuLys (Loai=DonViPhoiHop) → được
    /// </summary>
    public static Expression<Func<DuAnBuoc, bool>> BuildOwnershipFilter(long userId, long? phongBanId)
    {
        var phongBanIdValue = phongBanId ?? 0;
        var param = Expression.Parameter(typeof(DuAnBuoc), "b");

        // Điều kiện 1: User là Lãnh Đạo Phụ Trách DuAn
        var isLanhDao = BuildLanhDaoCondition(param, userId);

        // Điều kiện 2: User là người tạo bước
        var isCreator = BuildCreatorCondition(param, userId);

        // Điều kiện 3: Phòng ban phụ trách chính
        var isPhongBanChinh = BuildPhongBanChinhCondition(param, phongBanIdValue);

        // Điều kiện 4: Trong danh sách phối hợp AND thuộc DuAn.ChiuTrachNhiemXuLys
        var isPhoiHopInScope = BuildPhoiHopInChiuTrachNhiemScopeCondition(param, phongBanIdValue);

        // Combine: IsLanhDao || IsCreator || IsPhongBanChinh || IsPhoiHopInScope
        var combinedBody = Expression.OrElse(
            Expression.OrElse(isLanhDao, isCreator),
            Expression.OrElse(isPhongBanChinh, isPhoiHopInScope));

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
        // LanhDaoPhuTrachId is long? — constant must be typed as long? to match the property
        // and avoid System.InvalidOperationException on null DB values.
        var duAnProperty = Expression.Property(param, "DuAn");
        var lanhDaoProperty = Expression.Property(duAnProperty, "LanhDaoPhuTrachId");
        var nullCheck = Expression.NotEqual(duAnProperty, Expression.Constant(null, typeof(DuAn)));
        var compareCheck = Expression.Equal(lanhDaoProperty, Expression.Constant(userId, typeof(long?)));
        return Expression.AndAlso(nullCheck, compareCheck);
    }

    private static BinaryExpression BuildCreatorCondition(ParameterExpression param, long userId)
    {
        // b.CreatedBy == userId.ToString()
        var createdByProperty = Expression.Property(param, "CreatedBy");
        return Expression.Equal(createdByProperty, Expression.Constant(userId.ToString()));
    }

    private static BinaryExpression BuildPhongBanChinhCondition(ParameterExpression param, long phongBanId)
    {
        // b.PhongPhuTrachChinhId == phongBanIdValue
        // PhongPhuTrachChinhId is long? — constant must be typed as long? to match the property
        // and avoid System.InvalidOperationException on null DB values.
        var property = Expression.Property(param, "PhongPhuTrachChinhId");
        return Expression.Equal(property, Expression.Constant(phongBanId, typeof(long?)));
    }

    /// <summary>
    /// Điều kiện 4 mới: User nằm trong DuAnBuocPhongBanPhoiHops AND thuộc DuAn.DuAnChiuTrachNhiemXuLys (Loai=DonViPhoiHop).
    /// </summary>
    private static BinaryExpression BuildPhoiHopInChiuTrachNhiemScopeCondition(ParameterExpression param, long phongBanId)
    {
        // b.DuAnBuocPhongBanPhoiHops != null && b.DuAnBuocPhongBanPhoiHops.Any(p => p.RightId == phongBanIdValue)
        // AND b.DuAn != null
        // AND b.DuAn.DuAnChiuTrachNhiemXuLys!.Any(x => x.RightId == phongBanIdValue && x.Loai == DonViPhoiHop)
        var phongBanPhoiHops = Expression.Property(param, "DuAnBuocPhongBanPhoiHops");
        var nullPhoiHopCheck = Expression.NotEqual(phongBanPhoiHops, Expression.Constant(null, typeof(ICollection<DuAnBuocPhongBanPhoiHop>)));
        var phoiHopMatch = BuildAnyCall(phongBanPhoiHops, phongBanId);
        var phoiHopCondition = Expression.AndAlso(nullPhoiHopCheck, phoiHopMatch);

        var duAn = Expression.Property(param, "DuAn");
        var notNullDuAn = Expression.NotEqual(duAn, Expression.Constant(null, typeof(DuAn)));

        var chiuTrachNhiem = Expression.Property(duAn, "DuAnChiuTrachNhiemXuLys");
        var chiuTrachNhiemMatch = BuildAnyChiuTrachNhiemWithLoaiCall(chiuTrachNhiem, phongBanId, EChiuTrachNhiemXuLy.DonViPhoiHop);

        return Expression.AndAlso(phoiHopCondition, Expression.AndAlso(notNullDuAn, chiuTrachNhiemMatch));
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

    private static MethodCallExpression BuildAnyChiuTrachNhiemWithLoaiCall(Expression collection, long phongBanId, EChiuTrachNhiemXuLy loai)
    {
        // collection.Any(x => x.RightId == phongBanIdValue && x.Loai == loai)
        var anyMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(DuAnChiuTrachNhiemXuLy));

        var param = Expression.Parameter(typeof(DuAnChiuTrachNhiemXuLy), "x");
        var rightIdProperty = Expression.Property(param, "RightId");
        var loaiProperty = Expression.Property(param, "Loai");
        var rightIdCondition = Expression.Equal(rightIdProperty, Expression.Constant(phongBanId));
        var loaiCondition = Expression.Equal(loaiProperty, Expression.Constant(loai));
        var combined = Expression.AndAlso(rightIdCondition, loaiCondition);
        var lambda = Expression.Lambda(combined, param);

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
/// - CanExecuteStep (write): HasAdminCatalog OR ownership (Owner/LanhDao/PhongBanChinh/PhoiHopInScope)
/// - FilterVisibleSteps: filter qua ownership scope
/// - FilterVisibleChildEntities: filter child entity qua subquery visible buoc ids
/// - CanManageViewerListAsync: chỉ Owner/LanhDao/HasAdminCatalog (KHÔNG bao gồm PhongBanChinh/PhoiHop) — cho phép chỉnh DanhSachPhongBanPhoiHopIds
/// - CanManageStepFieldsAsync: chỉ Owner/LanhDao/HasAdminCatalog — cho phép edit/delete các field của bước
/// - CanExecuteThanhToanAsync: chỉ Owner/LanhDao/HasAdminCatalog + PhongBanChinh (KHÔNG cho PhoiHop) — cho phép Insert/Update ThanhToan
///
/// Authorization flags (HasAdminCatalog) are computed once per request
/// by AuthorizationContext and exposed via IAuthorizationContext parameter.
/// </summary>
public class BuocAuthorizationProvider(IRepository<DuAnBuoc, int> buocRepo) : IBuocAuthorizationProvider
{
    public async Task<bool> CanExecuteStepAsync(DuAnBuoc buoc, IAuthorizationContext ctx, CancellationToken ct)
    {
        if (ctx.HasAdminCatalog) return true;
        // HasReadAllBypass: không tự bypass write — ownership check phía sau quyết định.
        // NVTT_XemDuAn user khi assign Buoc sẽ match ownership → CUD được.

        return BuocAuthorizationHelper.CheckOwnership(buoc, ctx.UserId, ctx.PhongBanId);
    }

    public IQueryable<DuAnBuoc> FilterVisibleSteps(IQueryable<DuAnBuoc> query, IAuthorizationContext ctx)
    {
        if (ctx.HasAdminCatalog) return query;
        if (ctx.HasReadAllBypass) return query;

        if (ctx.PhongBanId == 0 && ctx.UserId <= 0)
            return query.Where(e => false);

        var filter = BuocAuthorizationHelper.BuildOwnershipFilter(ctx.UserId, ctx.PhongBanId);
        return query.Where(filter);
    }

    public IQueryable<T> FilterVisibleChildEntities<T>(
        IQueryable<T> query,
        IRepository<DuAnBuoc, int> buocRepository,
        IAuthorizationContext ctx,
        Expression<Func<T, int?>> buocIdSelector) where T : class
    {
        if (ctx.HasAdminCatalog) return query;
        if (ctx.HasReadAllBypass) return query;

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
            throw new ForbiddenException("Phòng ban không có quyền thao tác bước này");
    }

    /// <summary>
    /// Quyền chỉnh sửa DanhSachPhongBanPhoiHopIds: chỉ Owner (CreatedBy) + Lãnh đạo phụ trách + HasAdminCatalog.
    /// PhongBanChinh và PhongBanPhoiHop KHÔNG có quyền chỉnh viewer list.
    /// </summary>
    public async Task<bool> CanManageViewerListAsync(DuAnBuoc buoc, IAuthorizationContext ctx, CancellationToken ct)
    {
        return await CanManageStepFieldsAsync(buoc, ctx, ct);
    }

    /// <summary>
    /// Throw ManagedException nếu user không có quyền chỉnh DanhSachPhongBanPhoiHopIds.
    /// </summary>
    public async Task EnsureCanManageViewerListAsync(int buocId, IAuthorizationContext ctx, CancellationToken ct = default)
    {
        var buoc = await buocRepo.GetQueryableSet()
            .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == buocId, ct);

        if (buoc == null) return;
        if (!await CanManageViewerListAsync(buoc, ctx, ct))
            throw new ForbiddenException("Chỉ Lãnh đạo phụ trách hoặc người tạo bước mới được chỉnh sửa danh sách phòng ban phối hợp");
    }

    /// <summary>
    /// Quyền edit/delete các field của bước (TenBuoc, Ngay, ManHinh, PhongPhuTrachChinhId): chỉ Owner + Lãnh đạo + HasAdminCatalog.
    /// </summary>
    public async Task<bool> CanManageStepFieldsAsync(DuAnBuoc buoc, IAuthorizationContext ctx, CancellationToken ct)
    {
        if (ctx.HasAdminCatalog) return true;

        if (buoc.CreatedBy == ctx.UserId.ToString()) return true;

        var lanhDaoId = buoc.DuAn?.LanhDaoPhuTrachId
            ?? await ctx.GetLanhDaoPhuTrachIdAsync(buoc.DuAnId, ct);
        if (lanhDaoId.HasValue && lanhDaoId.Value == ctx.UserId) return true;

        return false;
    }

    /// <summary>
    /// Throw ManagedException nếu user không có quyền edit/delete các field của bước.
    /// Noop khi buocId null.
    /// </summary>
    public async Task EnsureCanManageStepFieldsAsync(int? buocId, IAuthorizationContext ctx, CancellationToken ct = default)
    {
        if (!buocId.HasValue) return;
        var buoc = await buocRepo.GetQueryableSet()
            .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == buocId.Value, ct);
        if (buoc == null) return;
        if (!await CanManageStepFieldsAsync(buoc, ctx, ct))
            throw new ForbiddenException("Chỉ Lãnh đạo phụ trách hoặc người tạo bước mới được chỉnh sửa thông tin bước");
    }

    /// <summary>
    /// Quyền Insert/Update ThanhToan: Owner + Lãnh đạo + HasAdminCatalog + PhongBanChinh.
    /// PhongBanPhoiHop KHÔNG có quyền (kể cả khi thuộc DuAn.ChiuTrachNhiemXuLys).
    /// </summary>
    public async Task<bool> CanExecuteThanhToanAsync(DuAnBuoc buoc, IAuthorizationContext ctx, CancellationToken ct)
    {
        if (await CanManageStepFieldsAsync(buoc, ctx, ct)) return true;

        // PhongBanChinh được Insert/Update ThanhToan nhưng KHÔNG được Delete
        return buoc.PhongPhuTrachChinhId == ctx.PhongBanId;
    }

    /// <summary>
    /// Throw ManagedException nếu user không có quyền Insert/Update ThanhToan.
    /// </summary>
    public async Task EnsureCanExecuteThanhToanAsync(int? buocId, IAuthorizationContext ctx, CancellationToken ct = default)
    {
        if (!buocId.HasValue) return;
        var buoc = await buocRepo.GetQueryableSet()
            .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == buocId.Value, ct);
        if (buoc == null) return;
        if (!await CanExecuteThanhToanAsync(buoc, ctx, ct))
            throw new ForbiddenException("Phòng ban không có quyền thao tác thanh toán");
    }

    private static IQueryable<T> ApplyChildBuocIdFilter<T>(
        IQueryable<T> query,
        IQueryable<int> visibleBuocIds,
        Expression<Func<T, int?>> buocIdSelector) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");

        // Inline the selector body against the unified query parameter. Invoking
        // a delegate constant (Expression.Invoke of a Func) is NOT EF-translatable;
        // a member-access expression tree is. This mirrors the working pattern in
        // DuAnAuthorizationProvider.ApplyChildDuAnIdFilter.
        var buocIdProperty = ParameterReplacer.Replace(
            buocIdSelector.Body, buocIdSelector.Parameters[0], parameter);

        // Coerce int? → int so Queryable.Contains<int> matches. Null branch is
        // short-circuited by the leading `buocIdProperty == null` OR clause, so
        // the Coalesce fallback (0) is never observed at runtime. SQL Server
        // IDENTITY seeds start at 1, so 0 cannot collide with a real DuAnBuoc.Id.
        var intSelector = Expression.Coalesce(buocIdProperty, Expression.Constant(0, typeof(int)));

        var containsMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(int));

        // e => visibleBuocIds.Contains(buocIdSelector(e) ?? 0)
        var containsCall = Expression.Call(
            containsMethod,
            visibleBuocIds.Expression,
            intSelector);

        // e => buocIdSelector(e) == null || visibleBuocIds.Contains(buocIdSelector(e) ?? 0)
        var nullCheck = Expression.Equal(buocIdProperty, Expression.Constant(null, buocIdProperty.Type));
        var combinedCondition = Expression.OrElse(nullCheck, containsCall);

        var lambda = Expression.Lambda<Func<T, bool>>(combinedCondition, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// Replaces a parameter in an expression body so a selector lambda can be
    /// inlined against a unified query parameter (EF-translatable member access
    /// rather than an opaque delegate Invoke).
    /// </summary>
    private sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam = oldParam;
        private readonly ParameterExpression _newParam = newParam;

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _oldParam ? _newParam : base.VisitParameter(node);

        public static Expression Replace(Expression body, ParameterExpression oldParam, ParameterExpression newParam)
            => new ParameterReplacer(oldParam, newParam).Visit(body);
    }
}
