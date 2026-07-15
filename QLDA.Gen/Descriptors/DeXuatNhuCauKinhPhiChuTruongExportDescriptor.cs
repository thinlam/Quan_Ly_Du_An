using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class DeXuatNhuCauKinhPhiChuTruongExportDescriptor : IExportDescriptor {
    public string EntityName => "Đề xuất nhu cầu kinh phí chủ trương";
    public string TemplateFileName => "DeXuatNhuCauKinhPhiChuTruong.xlsx";
    public string OutputPath { get; set; } = "";
    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;

    public bool HandMaintainedTemplate => true; // Template được giữ nguyên bởi người dùng.

    public string? Title => "ĐỀ XUẤT NHU CẦU KINH PHÍ CHỦ TRƯƠNG ĐẦU TƯ";

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT"),
        new("TrichYeu", "Trích yếu"),
        new("KinhPhiDeXuat", "Kinh phí đề xuất (đ)"),
        new("TenPhongDeXuat", "Phòng đề xuất"),
        new("SoPhieuChuyen", "Số phiếu chuyển"),
        new("TenTrangThai", "Trạng thái"),
    ];
}
