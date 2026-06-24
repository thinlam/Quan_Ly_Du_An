using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Descriptor for "DanhSachDuAn.xlsx".
/// Layout: SimpleLetterheadExport — 1-row letterhead + title (R2) + blank (R3) + display headers (R4, bold, gray fill, thin border)
/// + $Field template row (R5, thin border).
/// Template row = R5 (row index 5 in the file, 0-based data row).
/// Field names are camelCase to match the original template's binding conventions.
/// </summary>
public class DanhSachDuAnExportDescriptor : IExportDescriptor
{
    public string EntityName => "DanhSachDuAn";
    public string TemplateFileName => "DanhSachDuAn.xlsx";
    public List<ExportColumn> Columns { get; } =
    [
        new("stt",                   "STT",                   6),
        new("maDuAn",                "Mã dự án",              12),
        new("tenDuAn",               "Tên dự án",             42, null, wrapText: true),
        new("thoiGianKhoiCong",      "Thời gian khởi công",   24),
        new("ngayBatDau",            "Ngày bắt đầu",           30),
        new("lanhDaoPhuTrachId",     "Lãnh đạo phụ trách",    30),
        new("donViPhuTrachChinhId",  "Đơn vị phụ trách",      28),
        new("donViPhoiHopIds",       "Đơn vị phối hợp",       25, null, wrapText: true),
        new("hinhThucDauTuId",       "Hình thức đầu tư",       22),
        new("hinhThucQuanLyDuAnId",  "Hình thức quản lý",     22),
        new("tongMucDauTu",          "Tổng mức đầu tư",       26, "#,##0"),
    ];

    public string OutputPath { get; set; } = string.Empty;

    public string Title => "DANH SÁCH DỰ ÁN";

    public TemplateLayoutType Layout => TemplateLayoutType.SimpleLetterheadExport;

    /// <summary>
    /// One-line letterhead matching the original template.
    /// </summary>
    public string LetterheadText => "Trung tâm chuyển đổi số Tp Hồ Chí Minh";
}
