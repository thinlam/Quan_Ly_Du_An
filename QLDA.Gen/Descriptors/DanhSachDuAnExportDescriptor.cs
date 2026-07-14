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

    public bool HandMaintainedTemplate => true; // Template được giữ nguyên bởi người dùng.

    public List<ExportColumn> Columns { get; } =
    [
        new("stt", "STT"),
        new("maDuAn", "Mã dự án"),
        new("tenDuAn", "Tên dự án"),
        new("thoiGianKhoiCong", "Thời gian khởi công"),
        new("lanhDaoPhuTrachId", "Lãnh đạo phụ trách"),
        new("donViPhuTrachChinhId", "Đơn vị phụ trách"),
        new("donViPhoiHopIds", "Đơn vị phối hợp"),
        new("hinhThucDauTuId", "Hình thức đầu tư"),
        new("hinhThucQuanLyDuAnId", "Hình thức quản lý"),
        new("tongMucDauTu", "Tổng mức đầu tư"),
    ];
}
