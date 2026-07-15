using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

/// <summary>
/// Descriptor for "DanhSachTongHopVanBanQuyetDinh.xlsx".
/// Layout: LetterheadExport — UBND letterhead (R1-R2) + title (R3) + blue headers (R4) + $Field row (R5).
/// Same format as DanhSachXinChuTruongDauTu.xlsx / DeXuatNhuCauKinhPhiChuTruong.xlsx.
/// </summary>
public class DanhSachTongHopVanBanQuyetDinhExportDescriptor : IExportDescriptor
{
    public string EntityName => "DanhSachTongHopVanBanQuyetDinh";
    public string TemplateFileName => "DanhSachTongHopVanBanQuyetDinh.xlsx";
    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT"),
        new("TenDuAn", "Dự án"),
        new("So", "Số văn bản"),
        new("Ngay", "Ngày"),
        new("Loai", "Loại văn bản"),
        new("TrichYeu", "Trích yếu"),
    ];

    public string OutputPath { get; set; } = string.Empty;

    public string Title => "TỔNG HỢP VĂN BẢN QUYẾT ĐỊNH";

    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;

    public bool HandMaintainedTemplate => true; // Template được giữ nguyên bởi người dùng.
}
