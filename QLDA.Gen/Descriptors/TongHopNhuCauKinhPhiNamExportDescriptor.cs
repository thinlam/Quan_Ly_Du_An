using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class TongHopNhuCauKinhPhiNamExportDescriptor : IExportDescriptor {
    public string EntityName => "Tổng hợp nhu cầu kinh phí năm";
    public string TemplateFileName => "TongHopNhuCauKinhPhiNam.xlsx";
    public string OutputPath { get; set; } = "";
    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;
    public string? Title => "TỔNG HỢP NHU CẦU KINH PHÍ NĂM";

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 6),
        new("SoPhieuChuyen", "Số phiếu chuyển", 28),
        new("PhongPctTrinh", "Phòng PCT trình", 35),
        new("PhongKhtcTongHop", "Phòng KH-TC tổng hợp", 35),
        new("PhongBgdPheDuyet", "Phòng BGĐ phê duyệt", 35),
    ];
}
