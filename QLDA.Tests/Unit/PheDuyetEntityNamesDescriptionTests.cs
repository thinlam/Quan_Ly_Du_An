using FluentAssertions;
using QLDA.Domain.Constants;
using Xunit;

namespace QLDA.Tests.Unit;

public class PheDuyetEntityNamesDescriptionTests
{
    [Theory]
    [InlineData(PheDuyetEntityNames.DeXuatChuTruongChuyenTiep, "Đề xuất chủ trương chuyển tiếp")]
    [InlineData("DeXuatChuTruongChuyenTiep", "Đề xuất chủ trương chuyển tiếp")]
    [InlineData(PheDuyetEntityNames.PheDuyetDuToan, "Phê duyệt dự toán")]
    public void GetDescriptionFromName_ResolvesByConstValueOrFieldName(string input, string expected)
    {
        input.GetDescriptionFromName().Should().Be(expected);
    }

    [Fact]
    public void GetDescriptionFromName_Unknown_ReturnsInput()
    {
        "UnknownEntity".GetDescriptionFromName().Should().Be("UnknownEntity");
    }
}
