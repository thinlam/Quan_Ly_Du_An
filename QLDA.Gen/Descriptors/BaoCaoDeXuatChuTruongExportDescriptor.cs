using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class BaoCaoDeXuatChuTruongExportDescriptor : IExportDescriptor {
    public string EntityName => "Báo cáo đề xuất chủ trương";
    public string TemplateFileName => "BaoCaoDeXuatChuTruong.xlsx";
    public string OutputPath { get; set; } = "";
    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExportWithSummary;
    public string? Title => "BÁO CÁO ĐỀ XUẤT CHỦ TRƯƠNG";

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 6),
        new("LoaiDeXuat", "Loại đề xuất", 22),
        new("TenDuAn", "Tên dự án", 50),
        new("PhongBanPhuTrach", "Phòng ban phụ trách", 35),
    ];
}
