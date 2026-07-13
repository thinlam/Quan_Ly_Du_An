using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Descriptor for "DanhSachQuanLyPheDuyet.xlsx" — LetterheadExport.
/// Template row = R5. Field names match <see cref="QLDA.Application.QuanLyPheDuyet.DTOs.PheDuyetExportDto"/>.
/// </summary>
public class DanhSachQuanLyPheDuyetExportDescriptor : IExportDescriptor
{
    public string EntityName => "DanhSachQuanLyPheDuyet";
    public string TemplateFileName => "DanhSachQuanLyPheDuyet.xlsx";
    public string OutputPath { get; set; } = string.Empty;

    public string? Title => "DANH SÁCH PHÊ DUYỆT";

    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 8, null, false, ColumnAlign.Center),
        new("TenDuAn", "Dự án", 36, null, true, ColumnAlign.Left),
        new("TenGiaiDoan", "Giai đoạn", 24, null, true, ColumnAlign.Left),
        new("TenBuoc", "Tên bước", 28, null, true, ColumnAlign.Left),
        new("NguoiTrinh", "Người trình", 24, null, true, ColumnAlign.Left),
        new("NguoiDuyet", "Người duyệt", 24, null, true, ColumnAlign.Left),
        new("TenTrangThai", "Trạng thái", 18, null, true, ColumnAlign.Center),
    ];
}
