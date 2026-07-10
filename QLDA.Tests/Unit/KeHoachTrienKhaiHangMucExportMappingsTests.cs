using FluentAssertions;
using QLDA.Application.KeHoachTrienKhaiHangMucs;
using QLDA.Domain.Entities;
using Xunit;

namespace QLDA.Tests.Unit;

public class KeHoachTrienKhaiHangMucExportMappingsTests
{
    [Fact]
    public void ToExportRows_ResolvesCanBoByUserPortalId()
    {
        const int giaiDoanId = 1;
        const long portalId = 50_001;

        var hangMucs = new List<HangMucKeHoach>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenHangMuc = "HM export",
                GiaiDoanId = giaiDoanId,
                CanBoChuTriId = portalId,
                CreatedAt = DateTimeOffset.UtcNow,
            },
        };

        var rows = KeHoachTrienKhaiHangMucExportMappings.ToExportRows(
            hangMucs,
            new Dictionary<int, string> { [giaiDoanId] = "Giai đoạn A" },
            new Dictionary<int, int> { [giaiDoanId] = 1 },
            new Dictionary<long, string>(),
            new Dictionary<long, string> { [portalId] = "Đào Thị Bích Tuyền" });

        rows.Single(r => !r.IsGroupHeader).CanBoChuTri.Should().Be("Đào Thị Bích Tuyền");
    }

    [Fact]
    public void ToExportRows_ResolvesCanBoPhoiHopByUserPortalId()
    {
        const int giaiDoanId = 1;
        const long portalId = 50_002;

        var hangMucs = new List<HangMucKeHoach>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenHangMuc = "HM export",
                GiaiDoanId = giaiDoanId,
                CanBoPhoiHopIds = [portalId],
                CreatedAt = DateTimeOffset.UtcNow,
            },
        };

        var rows = KeHoachTrienKhaiHangMucExportMappings.ToExportRows(
            hangMucs,
            new Dictionary<int, string> { [giaiDoanId] = "Giai đoạn A" },
            new Dictionary<int, int> { [giaiDoanId] = 1 },
            new Dictionary<long, string>(),
            new Dictionary<long, string> { [portalId] = "Đặng Trung Nghĩa" });

        rows.Single(r => !r.IsGroupHeader).CanBoPhoiHop.Should().Be("Đặng Trung Nghĩa");
    }

    [Fact]
    public void ToExportRows_MapsDateOnlyValues()
    {
        const int giaiDoanId = 1;

        var hangMucs = new List<HangMucKeHoach>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenHangMuc = "HM có ngày",
                GiaiDoanId = giaiDoanId,
                NgayBatDau = new DateOnly(2026, 7, 8),
                NgayKetThuc = new DateOnly(2026, 7, 28),
                CreatedAt = DateTimeOffset.UtcNow,
            },
        };

        var rows = KeHoachTrienKhaiHangMucExportMappings.ToExportRows(
            hangMucs,
            new Dictionary<int, string> { [giaiDoanId] = "Giai đoạn A" },
            new Dictionary<int, int> { [giaiDoanId] = 1 },
            new Dictionary<long, string>(),
            new Dictionary<long, string>());

        var item = rows.Single(r => !r.IsGroupHeader);
        item.NgayBatDau.Should().Be(new DateOnly(2026, 7, 8));
        item.NgayKetThuc.Should().Be(new DateOnly(2026, 7, 28));
    }

    [Fact]
    public void ToExportRows_NullDates_ReturnNull()
    {
        const int giaiDoanId = 1;

        var hangMucs = new List<HangMucKeHoach>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenHangMuc = "HM không ngày",
                GiaiDoanId = giaiDoanId,
                CreatedAt = DateTimeOffset.UtcNow,
            },
        };

        var rows = KeHoachTrienKhaiHangMucExportMappings.ToExportRows(
            hangMucs,
            new Dictionary<int, string> { [giaiDoanId] = "Giai đoạn A" },
            new Dictionary<int, int> { [giaiDoanId] = 1 },
            new Dictionary<long, string>(),
            new Dictionary<long, string>());

        var item = rows.Single(r => !r.IsGroupHeader);
        item.NgayBatDau.Should().BeNull();
        item.NgayKetThuc.Should().BeNull();
    }

    [Fact]
    public void ToExportRows_OrdersGroupsByProjectSort_NotGlobalName()
    {
        const int giaiDoanXin = 1;
        const int giaiDoanCb = 2;

        var hangMucs = new List<HangMucKeHoach>
        {
            CreateHangMuc("HM-3", giaiDoanCb, order: 3),
            CreateHangMuc("HM-1", giaiDoanXin, order: 1),
            CreateHangMuc("HM-2", giaiDoanXin, order: 2),
        };

        var sortById = new Dictionary<int, int>
        {
            [giaiDoanXin] = 1,
            [giaiDoanCb] = 2,
        };

        var tenById = new Dictionary<int, string>
        {
            [giaiDoanXin] = "Xin chủ trương",
            [giaiDoanCb] = "Chuẩn bị thực hiện đầu tư",
        };

        var rows = KeHoachTrienKhaiHangMucExportMappings.ToExportRows(
            hangMucs, tenById, sortById,
            new Dictionary<long, string>(),
            new Dictionary<long, string>());

        rows
            .Where(r => r.IsGroupHeader)
            .Select(r => r.GiaiDoan)
            .Should()
            .ContainInOrder("Xin chủ trương", "Chuẩn bị thực hiện đầu tư");
    }

    [Fact]
    public void ToExportRows_PreservesItemOrderWithinGroup()
    {
        const int giaiDoanId = 10;
        var hangMucs = new List<HangMucKeHoach>
        {
            CreateHangMuc("C", giaiDoanId, order: 1),
            CreateHangMuc("A", giaiDoanId, order: 2),
            CreateHangMuc("B", giaiDoanId, order: 3),
        };

        var rows = KeHoachTrienKhaiHangMucExportMappings.ToExportRows(
            hangMucs,
            new Dictionary<int, string> { [giaiDoanId] = "Giai đoạn A" },
            new Dictionary<int, int> { [giaiDoanId] = 1 },
            new Dictionary<long, string>(),
            new Dictionary<long, string>());

        rows.Where(r => !r.IsGroupHeader)
            .Select(r => r.TenHangMuc)
            .Should()
            .ContainInOrder("C", "A", "B");
    }

    private static HangMucKeHoach CreateHangMuc(string ten, int giaiDoanId, int order) =>
        new()
        {
            Id = Guid.NewGuid(),
            TenHangMuc = ten,
            GiaiDoanId = giaiDoanId,
            CreatedAt = new DateTimeOffset(2025, 1, order, 0, 0, 0, TimeSpan.Zero),
        };
}
