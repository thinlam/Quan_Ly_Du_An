using System.Linq.Expressions;
using System.Reflection;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;
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

    /// <summary>
    /// Bước chưa gán PhongPhuTrachChinhId và chưa có PBPH → fallback theo
    /// DuAn.DonViPhuTrachChinhId → user ở đúng đơn vị phụ trách chính dự án → True.
    /// </summary>
    [Fact]
    public void CheckOwnership_BuocChuaGan_UserThuocDonViPhuTrachChinhDuAn_True()
    {
        const long userPhongBan = 100;          // phòng ban user thuộc
        const long duAnDonViPhuTrachChinh = 100; // trùng với phòng ban user

        var duAn = new DuAn
        {
            Id = Guid.NewGuid(),
            DonViPhuTrachChinhId = duAnDonViPhuTrachChinh,
            DuAnChiuTrachNhiemXuLys = new List<DuAnChiuTrachNhiemXuLy>()
        };
        var buoc = new DuAnBuoc
        {
            Id = 1,
            DuAnId = duAn.Id,
            DuAn = duAn,
            PhongPhuTrachChinhId = null,
            DuAnBuocPhongBanPhoiHops = new List<DuAnBuocPhongBanPhoiHop>()
        };

        var result = BuocAuthorizationHelper.CheckOwnership(buoc, userId: 999, phongBanId: userPhongBan);

        Assert.True(result);
    }

    /// <summary>
    /// Bước chưa gán → fallback theo DuAn.DuAnChiuTrachNhiemXuLys (Loai=DonViPhoiHop)
    /// → user thuộc một trong các đơn vị phối hợp → True.
    /// </summary>
    [Fact]
    public void CheckOwnership_BuocChuaGan_UserThuocChiuTrachNhiemXuLyDuAn_True()
    {
        const long userPhongBan = 200;
        const long otherPhongBan = 999;

        var duAn = new DuAn
        {
            Id = Guid.NewGuid(),
            DonViPhuTrachChinhId = otherPhongBan, // user KHÔNG thuộc đơn vị phụ trách chính
            DuAnChiuTrachNhiemXuLys = new List<DuAnChiuTrachNhiemXuLy>
            {
                new() { LeftId = Guid.NewGuid(), RightId = userPhongBan, Loai = EChiuTrachNhiemXuLy.DonViPhoiHop },
                new() { LeftId = Guid.NewGuid(), RightId = 300,           Loai = EChiuTrachNhiemXuLy.DonViPhoiHop }
            }
        };
        var buoc = new DuAnBuoc
        {
            Id = 1,
            DuAnId = duAn.Id,
            DuAn = duAn,
            PhongPhuTrachChinhId = null,
            DuAnBuocPhongBanPhoiHops = null // chưa load → null
        };

        var result = BuocAuthorizationHelper.CheckOwnership(buoc, userId: 999, phongBanId: userPhongBan);

        Assert.True(result);
    }

    /// <summary>
    /// Bước đã gán PhongPhuTrachChinhId = X (X != phòng user) → KHÔNG fallback
    /// sang DuAn dù user thuộc DuAn.DonViPhuTrachChinhId → False (ưu tiên ownership bước).
    /// </summary>
    [Fact]
    public void CheckOwnership_BuocDaGanPhongBanChinh_UserThuocDonViPhuTrachChinhDuAnNhungKhongThuocBuoc_False()
    {
        const long userPhongBan = 100;
        const long duAnDonViPhuTrachChinh = 100; // user thuộc DuAn
        const long buocPhongBanChinh = 999;      // nhưng bước đã gán cho phòng khác

        var duAn = new DuAn
        {
            Id = Guid.NewGuid(),
            DonViPhuTrachChinhId = duAnDonViPhuTrachChinh,
            DuAnChiuTrachNhiemXuLys = new List<DuAnChiuTrachNhiemXuLy>()
        };
        var buoc = new DuAnBuoc
        {
            Id = 1,
            DuAnId = duAn.Id,
            DuAn = duAn,
            PhongPhuTrachChinhId = buocPhongBanChinh,
            DuAnBuocPhongBanPhoiHops = new List<DuAnBuocPhongBanPhoiHop>()
        };

        var result = BuocAuthorizationHelper.CheckOwnership(buoc, userId: 999, phongBanId: userPhongBan);

        Assert.False(result);
    }

    /// <summary>
    /// Bước đã có PBPH nhưng phòng user không nằm trong danh sách,
    /// dù user thuộc DuAn.DuAnChiuTrachNhiemXuLys → False (ưu tiên ownership bước).
    /// </summary>
    [Fact]
    public void CheckOwnership_BuocDaGanPBPH_UserThuocDuAnNhungKhongTrongPBPH_False()
    {
        const long userPhongBan = 200;
        const long otherPhongBan = 999;

        var duAn = new DuAn
        {
            Id = Guid.NewGuid(),
            DonViPhuTrachChinhId = otherPhongBan,
            DuAnChiuTrachNhiemXuLys = new List<DuAnChiuTrachNhiemXuLy>
            {
                new() { LeftId = Guid.NewGuid(), RightId = userPhongBan, Loai = EChiuTrachNhiemXuLy.DonViPhoiHop }
            }
        };
        var buoc = new DuAnBuoc
        {
            Id = 1,
            DuAnId = duAn.Id,
            DuAn = duAn,
            PhongPhuTrachChinhId = null, // chưa gán → kích hoạt bypass branch
            DuAnBuocPhongBanPhoiHops = new List<DuAnBuocPhongBanPhoiHop>
            {
                // PBPH chỉ chứa phòng khác, không chứa userPhongBan → 4a fail
                new() { LeftId = 1, RightId = 777 }
            }
        };

        var result = BuocAuthorizationHelper.CheckOwnership(buoc, userId: 999, phongBanId: userPhongBan);

        Assert.False(result);
    }

    /// <summary>
    /// Bước chưa gán, user không thuộc DuAn (cả DonViPhuTrachChinhId lẫn ChiuTrachNhiemXuLys) → False.
    /// </summary>
    [Fact]
    public void CheckOwnership_BuocChuaGan_UserKhongThuocDuAn_False()
    {
        const long userPhongBan = 888;
        const long duAnDonViPhuTrachChinh = 100;
        const long buocPhongBanChinh = 999;

        var duAn = new DuAn
        {
            Id = Guid.NewGuid(),
            DonViPhuTrachChinhId = duAnDonViPhuTrachChinh,
            DuAnChiuTrachNhiemXuLys = new List<DuAnChiuTrachNhiemXuLy>
            {
                new() { LeftId = Guid.NewGuid(), RightId = 777, Loai = EChiuTrachNhiemXuLy.DonViPhoiHop }
            }
        };
        var buoc = new DuAnBuoc
        {
            Id = 1,
            DuAnId = duAn.Id,
            DuAn = duAn,
            PhongPhuTrachChinhId = buocPhongBanChinh,
            DuAnBuocPhongBanPhoiHops = new List<DuAnBuocPhongBanPhoiHop>()
        };

        var result = BuocAuthorizationHelper.CheckOwnership(buoc, userId: 999, phongBanId: userPhongBan);

        Assert.False(result);
    }

    private static MethodInfo OpenApplyChildBuocIdFilter()
    {
        var open = typeof(BuocAuthorizationProvider).GetMethod(
            "ApplyChildBuocIdFilter",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        return open.MakeGenericMethod(typeof(HopDong));
    }
}