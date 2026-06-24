using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class DeXuatNhuCauKinhPhiChuTruongExportDescriptor : IExportDescriptor {
    public string EntityName => "Đề xuất nhu cầu kinh phí chủ trương";
    public string TemplateFileName => "DeXuatNhuCauKinhPhiChuTruong.xlsx";
    public string OutputPath { get; set; } = "";
    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;
    public string? Title => "ĐỀ XUẤT NHU CẦU KINH PHÍ CHỦ TRƯƠNG ĐẦU TƯ";

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 6, null, false, ColumnAlign.Center),
        new("TrichYeu", "Trích yếu", 50, null, true, ColumnAlign.Left),
        new("KinhPhiDeXuat", "Kinh phí đề xuất (đ)", 22, "#,##0", false, ColumnAlign.Right),
        new("TenPhongDeXuat", "Phòng đề xuất", 35, null, false, ColumnAlign.Left),
        new("SoPhieuChuyen", "Số phiếu chuyển", 22, null, false, ColumnAlign.Left),
        new("TenTrangThai", "Trạng thái", 24, null, false, ColumnAlign.Left),
    ];
}
