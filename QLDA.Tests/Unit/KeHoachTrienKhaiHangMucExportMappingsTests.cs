using FluentAssertions;
using QLDA.Application.KeHoachTrienKhaiHangMucs;
using QLDA.Domain.Entities;
using Xunit;

namespace QLDA.Tests.Unit;

public class KeHoachTrienKhaiHangMucExportMappingsTests
{
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
