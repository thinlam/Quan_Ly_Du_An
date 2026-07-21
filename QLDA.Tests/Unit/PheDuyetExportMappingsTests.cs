using FluentAssertions;
using QLDA.Application.QuanLyPheDuyet;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using Xunit;

namespace QLDA.Tests.Unit;

public class PheDuyetExportMappingsTests
{
    [Fact]
    public void ToExportDtos_MapsTepDinhKem_FromOriginalName()
    {
        var rows = new List<PheDuyetListItemDto>
        {
            new()
            {
                TenDuAn = "DA1",
                DanhSachTepDinhKem =
                [
                    new TepDinhKemDto { OriginalName = "a.pdf", FileName = "stored-a.pdf" },
                    new TepDinhKemDto { OriginalName = "b.docx", FileName = "stored-b.docx" },
                ],
            },
        };

        var export = PheDuyetExportMappings.ToExportDtos(rows);

        export.Should().HaveCount(1);
        export[0].TepDinhKem.Should().Be($"a.pdf{Environment.NewLine}b.docx");
    }

    [Fact]
    public void FormatTepDinhKem_FallsBackToFileName_WhenOriginalNameMissing()
    {
        var files = new List<TepDinhKemDto>
        {
            new() { OriginalName = null, FileName = "only-stored.pdf" },
        };

        PheDuyetExportMappings.FormatTepDinhKem(files).Should().Be("only-stored.pdf");
    }

    [Fact]
    public void FormatTepDinhKem_EmptyOrNull_ReturnsEmptyString()
    {
        PheDuyetExportMappings.FormatTepDinhKem(null).Should().BeEmpty();
        PheDuyetExportMappings.FormatTepDinhKem([]).Should().BeEmpty();
    }

    [Fact]
    public void ToExportDtos_NoAttachments_TepDinhKemEmpty_NotNull()
    {
        var export = PheDuyetExportMappings.ToExportDtos(
        [
            new PheDuyetListItemDto { DanhSachTepDinhKem = [] },
        ]);

        export[0].TepDinhKem.Should().BeEmpty();
    }
}
