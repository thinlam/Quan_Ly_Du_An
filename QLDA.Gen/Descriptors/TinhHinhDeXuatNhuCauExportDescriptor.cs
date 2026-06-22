using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class TinhHinhDeXuatNhuCauExportDescriptor : IExportDescriptor {
    public string EntityName => "Tình hình đề xuất nhu cầu";
    public string TemplateFileName => "TinhHinhDeXuatNhuCau.xlsx";
    public string OutputPath { get; set; } = "";
    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;
    public string? Title => "TÌNH HÌNH ĐỀ XUẤT NHU CẦU";

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 6),
        new("SoPhieuChuyen", "Số phiếu chuyển", 28),
        new("PhongPctTrinh", "Phòng PCT trình", 35),
        new("PhongKhtcTongHop", "Phòng KH-TC tổng hợp", 35),
        new("PhongBgdPheDuyet", "Phòng BGĐ phê duyệt", 35),
    ];
}
