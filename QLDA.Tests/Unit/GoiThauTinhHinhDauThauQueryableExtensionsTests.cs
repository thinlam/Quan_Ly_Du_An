using FluentAssertions;
using QLDA.Application.GoiThaus;
using QLDA.Domain.Entities;
using Xunit;

namespace QLDA.Tests.Unit;

public class GoiThauTinhHinhDauThauQueryableExtensionsTests
{
    private static readonly Guid MatchingDuAnId = Guid.Parse("08dec1fd-220c-da70-687a-7b47980360c9");

    private static IQueryable<GoiThau> CreateSampleData() =>
        new List<GoiThau>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DuAnId = MatchingDuAnId,
                DuAn = new DuAn
                {
                    GiaiDoanHienTaiId = 22,
                    ThoiGianKhoiCong = 2026,
                    ThoiGianHoanThanh = 2028,
                    NgayBatDau = null,
                },
            },
            new()
            {
                Id = Guid.NewGuid(),
                DuAnId = Guid.NewGuid(),
                DuAn = new DuAn
                {
                    GiaiDoanHienTaiId = 10,
                    ThoiGianKhoiCong = 2025,
                    ThoiGianHoanThanh = 2025,
                },
            },
        }.AsQueryable();

    [Fact]
    public void ApplyFilters_WithAllParams_ReturnsSingleMatch()
    {
        var result = CreateSampleData()
            .ApplyTinhHinhDauThauFilters(MatchingDuAnId, giaiDoanId: 22)
            .ApplyTinhHinhDauThauNamDuAnFilter(2026)
            .ToList();

        result.Should().HaveCount(1);
        result[0].DuAnId.Should().Be(MatchingDuAnId);
    }

    [Fact]
    public void ApplyNamDuAnFilter_MatchesThoiGianKhoiCong_WhenNgayBatDauNull()
    {
        var result = CreateSampleData()
            .ApplyTinhHinhDauThauNamDuAnFilter(2026)
            .ToList();

        result.Should().HaveCount(1);
        result[0].DuAn!.NgayBatDau.Should().BeNull();
    }

    [Fact]
    public void ApplyNamFilter_RequiresNgayBatDau()
    {
        var result = CreateSampleData()
            .ApplyTinhHinhDauThauNamFilter(2026)
            .ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ApplyFilters_GiaiDoanIdMinusOne_DoesNotFilterByGiaiDoan()
    {
        var result = CreateSampleData()
            .ApplyTinhHinhDauThauFilters(duAnId: null, giaiDoanId: -1)
            .ToList();

        result.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 0)]
    public void ApplyTabFilter_FiltersByTrangThai(int trangThai, int expectedCount)
    {
        var result = CreateSampleData()
            .ApplyTinhHinhDauThauTabFilter(trangThai)
            .ToList();

        result.Should().HaveCount(expectedCount);
    }
}
