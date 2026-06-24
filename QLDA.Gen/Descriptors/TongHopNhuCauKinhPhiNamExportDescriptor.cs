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
        new("Stt", "STT", 6, null, false, ColumnAlign.Center),
        new("SoKeHoach", "Số kế hoạch", 28, null, false, ColumnAlign.Center),
        new("TrichYeu", "Trích yếu", 50, null, true, ColumnAlign.Left),
        new("TongHopChiPhi", "Tổng hợp chi phí", 20, "#,##0", false, ColumnAlign.Right),
        new("Ngay", "Ngày", 18, null, false, ColumnAlign.Right),
        new("TrangThai", "Trạng thái", 32, null, false, ColumnAlign.Left),
        new("SoLuongTepDinhKem", "Số lượng tệp đính kèm", 27, null, false, ColumnAlign.Right),
    ];
}
