using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;
using QLDA.Persistence;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

/// <summary>
/// EF Core translation check for BuocAuthorizationProvider.ApplyChildBuocIdFilter.
///
/// A bare DbContext cannot build the full QLDA model on SQLite (TepDinhKem
/// shared-table validation), but the WebApiFixture's DI-resolved DbContext can,
/// so this test goes through the fixture. It exercises the exact expression
/// that 500'd in production: ownership-filtered visible-buoc-ids subquery
/// passed into Contains against the child entity's int? BuocId selector.
/// </summary>
[Collection("WebApi")]
public class BuocAuthorizationProviderTranslationTests(WebApiFixture fixture)
{
    [Fact]
    public async Task ApplyChildBuocIdFilter_TranslatesOnEf()
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Same subquery shape FilterVisibleChildEntities builds (non-admin path).
        var visibleBuocIds = db.Set<DuAnBuoc>()
            .Where(BuocAuthorizationHelper.BuildOwnershipFilter(userId: 30, phongBanId: 999))
            .Select(b => b.Id);
        Expression<Func<HopDong, int?>> selector = e => e.BuocId;

        var method = typeof(BuocAuthorizationProvider)
            .GetMethod("ApplyChildBuocIdFilter", BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(typeof(HopDong));
        var filtered = (IQueryable<HopDong>)method.Invoke(
            null,
            [db.Set<HopDong>().AsQueryable(), visibleBuocIds, selector])!;

        // ToListAsync drives EF translation. Pre-fix threw
        // "...could not be translated" because of the Invoke(delegate) node.
        var ex = await Record.ExceptionAsync(() => filtered.Take(1).ToListAsync());
        Assert.Null(ex);
    }
}