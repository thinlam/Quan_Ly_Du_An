using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Descriptor for "DanhSachFileBanGiaoHoSo.xlsx" — LetterheadExport.
/// Used by ExportMultiLevelHierarchical (STT + Dự án merge, Tên file + Thời gian đính kèm per row).
/// Template row = R5.
/// </summary>
public class DanhSachFileBanGiaoHoSoExportDescriptor : IExportDescriptor
{
    public string EntityName => "DanhSachFileBanGiaoHoSo";
    public string TemplateFileName => "DanhSachFileBanGiaoHoSo.xlsx";
    public string OutputPath { get; set; } = string.Empty;

    public string? Title => "DANH SÁCH FILE BÀN GIAO HỒ SƠ";

    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 8, null, false, ColumnAlign.Center),
        new("TenDuAn", "Dự án", 36, null, true, ColumnAlign.Left),
        new("TenFile", "Tên file", 42, null, true, ColumnAlign.Left),
        new("ThoiGianDinhKem", "Thời gian đính kèm", 22, null, false, ColumnAlign.Center),
    ];
}
