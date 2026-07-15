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
                    NgayBatDau = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
                },
            },
            new()
            {
                Id = Guid.NewGuid(),
                DuAnId = Guid.NewGuid(),
                DuAn = new DuAn
                {
                    GiaiDoanHienTaiId = 10,
                    NgayBatDau = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero),
                },
            },
            new()
            {
                Id = Guid.NewGuid(),
                DuAnId = Guid.NewGuid(),
                DuAn = new DuAn
                {
                    GiaiDoanHienTaiId = 22,
                    NgayBatDau = null,
                },
            },
        }.AsQueryable();

    [Fact]
    public void ApplyFilters_WithAllParams_ReturnsSingleMatch()
    {
        var result = CreateSampleData()
            .ApplyTinhHinhDauThauFilters(MatchingDuAnId, giaiDoanId: 22)
            .ApplyTinhHinhDauThauNamFilter(2026)
            .ToList();

        result.Should().HaveCount(1);
        result[0].DuAnId.Should().Be(MatchingDuAnId);
    }

    [Fact]
    public void ApplyNamFilter_MatchesNgayBatDau_ForGivenYear()
    {
        // Đảm bảo API print (namDuAn) và API list (nam) trả cùng tập dữ liệu
        // vì dùng chung ApplyTinhHinhDauThauNamFilter.
        var result = CreateSampleData()
            .ApplyTinhHinhDauThauNamFilter(2026)
            .ToList();

        result.Should().HaveCount(1);
        result[0].DuAnId.Should().Be(MatchingDuAnId);
    }

    [Fact]
    public void ApplyNamFilter_ExcludesEntitiesWithNullNgayBatDau()
    {
        var result = CreateSampleData()
            .ApplyTinhHinhDauThauNamFilter(2026)
            .ToList();

        result.Should().NotContain(e => e.DuAn!.NgayBatDau == null);
    }

    [Fact]
    public void ApplyNamFilter_NullOrNonPositive_DoesNotFilter()
    {
        CreateSampleData().ApplyTinhHinhDauThauNamFilter(null).ToList().Should().HaveCount(3);
        CreateSampleData().ApplyTinhHinhDauThauNamFilter(0).ToList().Should().HaveCount(3);
        CreateSampleData().ApplyTinhHinhDauThauNamFilter(-1).ToList().Should().HaveCount(3);
    }

    [Fact]
    public void ApplyFilters_GiaiDoanIdMinusOne_DoesNotFilterByGiaiDoan()
    {
        var result = CreateSampleData()
            .ApplyTinhHinhDauThauFilters(duAnId: null, giaiDoanId: -1)
            .ToList();

        result.Should().HaveCount(3);
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
