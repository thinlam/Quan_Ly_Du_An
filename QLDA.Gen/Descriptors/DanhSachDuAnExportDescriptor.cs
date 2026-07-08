using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Descriptor for "DanhSachDuAn.xlsx".
/// Layout: LetterheadExport — UBND letterhead + title + blue table headers + $Field row
/// (cùng format với KeHoachTrienKhaiHangMuc.xlsx).
/// Template row = R5.
/// Placeholders: $stt, $maDuAn, $tenDuAn, $thoiGianKhoiCong, $lanhDaoPhuTrachId,
/// $donViPhuTrachChinhId, $donViPhoiHopIds, $hinhThucDauTuId, $hinhThucQuanLyDuAnId, $tongMucDauTu.
/// </summary>
public class DanhSachDuAnExportDescriptor : IExportDescriptor
{
    public string EntityName => "DanhSachDuAn";
    public string TemplateFileName => "DanhSachDuAn.xlsx";
    public string OutputPath { get; set; } = string.Empty;

    public string? Title => "DANH SÁCH DỰ ÁN";

    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;

    public List<ExportColumn> Columns { get; } =
    [
        new("stt",                   "STT",                   6, null, false, ColumnAlign.Center),
        new("maDuAn",                "Mã dự án",             12, null, false, ColumnAlign.Center),
        new("tenDuAn",               "Tên dự án",            42, null, true,  ColumnAlign.Left),
        new("thoiGianKhoiCong",      "Thời gian khởi công",  16, null, false, ColumnAlign.Center),
        new("lanhDaoPhuTrachId",     "Lãnh đạo phụ trách",   24, null, true,  ColumnAlign.Left),
        new("donViPhuTrachChinhId",  "Đơn vị phụ trách",     22, null, true,  ColumnAlign.Left),
        new("donViPhoiHopIds",       "Đơn vị phối hợp",      22, null, true,  ColumnAlign.Left),
        new("hinhThucDauTuId",       "Hình thức đầu tư",     20, null, true,  ColumnAlign.Left),
        new("hinhThucQuanLyDuAnId",  "Hình thức quản lý",    20, null, true,  ColumnAlign.Left),
        new("tongMucDauTu",          "Tổng mức đầu tư",      18, "#,##0", false, ColumnAlign.Right),
    ];
}
