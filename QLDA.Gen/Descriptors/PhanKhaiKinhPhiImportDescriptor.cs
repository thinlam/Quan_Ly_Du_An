using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class PhanKhaiKinhPhiImportDescriptor : IImportDescriptor {
    public string EntityName => "Import phân khai kinh phí";
    public string TemplateFileName => "Import_PhanKhaiKinhPhi.xlsx";
    public string TableName => "PhanKhaiKinhPhiImport";
    public string OutputPath { get; set; } = "";
    public string? Title => "MẪU IMPORT PHÂN KHAI KINH PHÍ";
    public string? HintText =>
        "Nhập dữ liệu vào bảng bên dưới. Cột Dự án / Nguồn vốn chọn từ danh sách. Nguồn vốn hiển thị dạng \"Tên nguồn vốn - Tên dự án\". Tiền nhập theo đồng.";

    public List<ImportColumn> Columns { get; } =
    [
        new() { Header = "Dự án", Description = "Chọn từ danh sách", Placeholder = "$cbo1", ComboIndex = 1, Width = 40 },
        new() { Header = "Nguồn vốn", Description = "Tên nguồn vốn - Tên dự án", Placeholder = "$cbo2", ComboIndex = 2, Width = 40 },
        new() { Header = "Kinh phí đề xuất", Description = "Số ≥ 0 (đồng)", NumberFormat = "#,##0", Width = 22 },
        new() { Header = "Kinh phí phân khai", Description = "Số ≥ 0 (đồng)", NumberFormat = "#,##0", Width = 22 },
        new() { Header = "Thuyết minh phân khai", Description = "Tùy chọn", Width = 35 },
        new() { Header = "Số tờ trình", Description = "Tùy chọn", Width = 18 },
        new() { Header = "Ngày tờ trình", Description = "dd/MM/yyyy", NumberFormat = "dd/MM/yyyy", Width = 16 },
        new() { Header = "Trích yếu", Description = "Tùy chọn", Width = 35 },
    ];
}
