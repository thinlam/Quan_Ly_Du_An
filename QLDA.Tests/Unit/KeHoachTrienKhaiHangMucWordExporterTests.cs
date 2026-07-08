using FluentAssertions;
using QLDA.Infrastructure.Offices;
using Xunit;

namespace QLDA.Tests.Unit;

public class KeHoachTrienKhaiHangMucWordExporterTests
{
    [Fact]
    public void FormatDate_UsesVietnameseShortDate()
    {
        var result = KeHoachTrienKhaiHangMucWordExporter.FormatDate(new DateTime(2026, 7, 8));

        result.Should().Be("08/07/2026");
    }

    [Fact]
    public void FormatDate_Null_ReturnsEmpty()
    {
        KeHoachTrienKhaiHangMucWordExporter.FormatDate(null).Should().BeEmpty();
    }
}
