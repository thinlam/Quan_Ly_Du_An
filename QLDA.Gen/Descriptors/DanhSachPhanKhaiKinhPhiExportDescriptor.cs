using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class DanhSachPhanKhaiKinhPhiExportDescriptor : IExportDescriptor {
    public string EntityName => "Danh sách phân khai kinh phí";
    public string TemplateFileName => "DanhSachPhanKhaiKinhPhi.xlsx";
    public string OutputPath { get; set; } = "";
    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;
    public string? Title => "DANH SÁCH PHÂN KHAI KINH PHÍ";

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 6),
        new("TenDuAn", "Dự án", 45),
        new("KinhPhiDeXuat", "KP đề xuất", 18, "#,##0"),
        new("KinhPhiPhanKhai", "KP phân khai", 18, "#,##0"),
        new("TenTrangThai", "Trạng thái", 22),
    ];
}
