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
        new("Stt",      "STT",           6,  null, false, ColumnAlign.Center),
        new("TenDuAn",  "Dự án",        36,  null, true,  ColumnAlign.Left),
        new("So",       "Số văn bản",   20,  null, false, ColumnAlign.Left),
        new("Ngay",     "Ngày",         32,  "dd/MM/yyyy", false, ColumnAlign.Center),
        new("Loai",     "Loại văn bản", 39,  null, false, ColumnAlign.Left),
        new("TrichYeu", "Trích yếu",    49,  null, true,  ColumnAlign.Left),
    ];

    public string OutputPath { get; set; } = string.Empty;

    public string Title => "TỔNG HỢP VĂN BẢN QUYẾT ĐỊNH";

    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;
}
