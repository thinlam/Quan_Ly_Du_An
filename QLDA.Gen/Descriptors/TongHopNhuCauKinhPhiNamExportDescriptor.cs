using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class TongHopNhuCauKinhPhiNamExportDescriptor : IExportDescriptor {
    public string EntityName => "Tổng hợp nhu cầu kinh phí năm";
    public string TemplateFileName => "TongHopNhuCauKinhPhiNam.xlsx";
    public string OutputPath { get; set; } = "";
    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;
    public string? Title => "TỔNG HỢP NHU CẦU KINH PHÍ NĂM";
    public bool HandMaintainedTemplate => true; // Template được giữ nguyên bởi người dùng.

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT"),
        new("SoKeHoach", "Số kế hoạch"),
        new("TrichYeu", "Trích yếu"),
        new("TongHopChiPhi", "Tổng hợp chi phí"),
        new("Ngay", "Ngày"),
        new("TrangThai", "Trạng thái"),
        new("SoLuongTepDinhKem", "Số lượng tệp đính kèm"),
    ];
}
