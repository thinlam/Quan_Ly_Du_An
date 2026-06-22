using System.Linq.Expressions;
using System.Reflection;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;
using Xunit;

namespace QLDA.Tests.Unit;

/// <summary>
/// Composition + semantics regression tests for
/// BuocAuthorizationProvider.ApplyChildBuocIdFilter. EF Core translation is
/// verified separately in Integration/BuocAuthorizationProviderTranslationTests
/// (requires the WebApiFixture's full model, which a bare DbContext cannot build).
/// </summary>
public class BuocAuthorizationProviderChildFilterTests
{
    [Fact]
    public void ApplyChildBuocIdFilter_NullableSelector_DoesNotThrow()
    {
        // Composition check (LINQ-to-Objects): pre-fix threw ArgumentException
        // at Expression.Call because Contains<int> rejected the int? selector.
        var query = new List<HopDong>().AsQueryable();
        var visibleBuocIds = new List<int>().AsQueryable();
        Expression<Func<HopDong, int?>> selector = e => e.BuocId;

        var method = OpenApplyChildBuocIdFilter();

        var exception = Record.Exception(() => method.Invoke(null, [query, visibleBuocIds, selector]));

        Assert.Null(exception);
    }

    [Fact]
    public void ApplyChildBuocIdFilter_NullBuocId_NotFilteredOut()
    {
        // A HopDong with BuocId=null passes the leading null check and survives,
        // even when the Coalesce fallback (0) is present in visibleBuocIds.
        var hopDongWithNullBuoc = new HopDong
        {
            Id = Guid.NewGuid(),
            DuAnId = Guid.NewGuid(),
            BuocId = null
        };
        var query = new List<HopDong> { hopDongWithNullBuoc }.AsQueryable();
        var visibleBuocIds = new List<int> { 0 }.AsQueryable();
        Expression<Func<HopDong, int?>> selector = e => e.BuocId;

        var method = OpenApplyChildBuocIdFilter();
        var result = (IQueryable<HopDong>)method.Invoke(null, [query, visibleBuocIds, selector])!;

        Assert.Single(result.ToList());
    }

    private static MethodInfo OpenApplyChildBuocIdFilter()
    {
        var open = typeof(BuocAuthorizationProvider).GetMethod(
            "ApplyChildBuocIdFilter",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        return open.MakeGenericMethod(typeof(HopDong));
    }
}