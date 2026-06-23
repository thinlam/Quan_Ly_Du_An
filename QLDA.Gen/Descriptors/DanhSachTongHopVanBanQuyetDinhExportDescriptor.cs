using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Descriptor for "DanhSachTongHopVanBanQuyetDinh.xlsx".
/// Layout: Standard6RowWithLetterhead — 2-row letterhead + title (R3) + blank (R4)
/// + display headers (R5, bold, thin border) + $Field template row (R6, thin border).
/// Template row = R6 (row index 6 in the file, 0-based data row).
/// </summary>
public class DanhSachTongHopVanBanQuyetDinhExportDescriptor : IExportDescriptor
{
    public string EntityName => "DanhSachTongHopVanBanQuyetDinh";
    public string TemplateFileName => "DanhSachTongHopVanBanQuyetDinh.xlsx";
    public List<ExportColumn> Columns { get; } =
    [
        new("stt",       "STT",           6),
        new("tenDuAn",   "Dự án",        30, null, wrapText: true),
        new("so",        "Số văn bản",   15),
        new("ngay",      "Ngày",          12),
        new("loai",      "Loại văn bản",  22),
        new("trichYeu",  "Trích yếu",     35, null, wrapText: true),
    ];

    public string OutputPath { get; set; } = string.Empty;

    public string Title => "VĂN BẢN - QUYẾT ĐỊNH";

    public TemplateLayoutType Layout => TemplateLayoutType.Standard6RowWithLetterhead;

    /// <summary>
    /// Two-line letterhead matching the original template.
    /// </summary>
    public string LetterheadText => "Trung tâm chuyển đổi số thành phố Hồ Chí Minh";
}
