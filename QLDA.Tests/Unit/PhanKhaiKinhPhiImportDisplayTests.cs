using FluentAssertions;
using QLDA.Application.PhanKhaiKinhPhis;
using Xunit;

namespace QLDA.Tests.Unit;

public class PhanKhaiKinhPhiImportDisplayTests {
    [Fact]
    public void Format_CombinesTenNguonVonAndTenDuAn() {
        PhanKhaiKinhPhiImportDisplay.Format("Ngân sách thành phố", "Dự án A")
            .Should().Be("Ngân sách thành phố - Dự án A");
    }

    [Fact]
    public void Parse_SplitsFromRight_WhenTenNguonVonContainsSeparator() {
        var (tenNv, tenDa) = PhanKhaiKinhPhiImportDisplay.Parse(
            "Ngân sách - thành phố - Dự án A");

        tenNv.Should().Be("Ngân sách - thành phố");
        tenDa.Should().Be("Dự án A");
    }

    [Fact]
    public void Parse_WithoutSeparator_ReturnsTenNguonVonOnly() {
        var (tenNv, tenDa) = PhanKhaiKinhPhiImportDisplay.Parse("Ngân sách thành phố");

        tenNv.Should().Be("Ngân sách thành phố");
        tenDa.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyInput_ReturnsNulls(string? input) {
        var (tenNv, tenDa) = PhanKhaiKinhPhiImportDisplay.Parse(input);

        tenNv.Should().BeNull();
        tenDa.Should().BeNull();
    }
}
