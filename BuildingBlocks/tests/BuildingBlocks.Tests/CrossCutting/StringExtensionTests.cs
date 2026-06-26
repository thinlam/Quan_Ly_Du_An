using System.Globalization;
using BuildingBlocks.CrossCutting.ExtensionMethods;
using Xunit;

namespace BuildingBlocks.Tests.CrossCutting;

public class StringExtensionTests
{
    [Theory]
    [InlineData("26/06/2025", 2025, 6, 26)]
    [InlineData("6/6/2025", 2025, 6, 6)]
    [InlineData("26-06-2025", 2025, 6, 26)]
    public void ConvertStringToPropertyType_DateOnly_parses_vn_formats(string input, int year, int month, int day)
    {
        var result = input.ConvertStringToPropertyType(typeof(DateOnly?));

        Assert.Equal(new DateOnly(year, month, day), result);
    }

    [Fact]
    public void ConvertStringToPropertyType_DateOnly_parses_excel_oadate_serial()
    {
        var oa = DateTime.FromOADate(45834).ToOADate().ToString(CultureInfo.InvariantCulture);

        var result = oa.ConvertStringToPropertyType(typeof(DateOnly?));

        Assert.Equal(DateOnly.FromDateTime(DateTime.FromOADate(45834)), result);
    }

    [Fact]
    public void ConvertStringToPropertyType_DateOnly_empty_returns_null()
    {
        var result = "".ConvertStringToPropertyType(typeof(DateOnly?));

        Assert.Null(result);
    }
}
