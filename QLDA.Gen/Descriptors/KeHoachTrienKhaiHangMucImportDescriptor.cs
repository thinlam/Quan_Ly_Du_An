using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class KeHoachTrienKhaiHangMucImportDescriptor : IImportDescriptor {
    public string EntityName => "Import kế hoạch triển khai hạng mục";
    public string TemplateFileName => "Import_KeHoachTrienKhaiHangMuc.xlsx";
    public string TableName => "KeHoachTrienKhaiHangMucImport";
    public string OutputPath { get; set; } = "";
    public string? Title => "MẪU IMPORT KẾ HOẠCH TRIỂN KHAI HẠNG MỤC";
    public string? HintText =>
        "Nhập dữ liệu vào bảng bên dưới. Dự án / Giai đoạn / Đơn vị / Cán bộ chọn từ danh sách. "
        + "Đơn vị phối hợp và Cán bộ phối hợp: chọn từ danh sách, nhiều giá trị cách nhau dấu phẩy (vd: Phòng A, Phòng B). Ngày nhập dd/MM/yyyy.";
    public List<ImportColumn> Columns { get; } =
    [
        new() { Header = "Dự án", Description = "Chọn từ danh sách", Placeholder = "$cbo1", ComboIndex = 1, Width = 40,
            HorizontalAlign = ColumnAlign.Left, WrapText = true, Required = true },
        new() { Header = "Tên hạng mục", Description = "Bắt buộc", Width = 40,
            HorizontalAlign = ColumnAlign.Left, WrapText = true, Required = true },
        new() { Header = "Giai đoạn", Description = "Chọn từ danh mục", Placeholder = "$cbo2", ComboIndex = 2, Width = 22,
            HorizontalAlign = ColumnAlign.Left, Required = true },
        new() { Header = "Đơn vị chủ trì", Description = "Chọn từ danh sách", Placeholder = "$cbo3", ComboIndex = 3, Width = 28,
            HorizontalAlign = ColumnAlign.Left, WrapText = true, Required = true },
        new() { Header = "Đơn vị phối hợp", Description = "Chọn từ danh sách", Placeholder = "$cbo5", ComboIndex = 5, Width = 32,
            HorizontalAlign = ColumnAlign.Left, WrapText = true, Required = true },
        new() { Header = "Cán bộ chủ trì", Description = "Chọn từ danh sách", Placeholder = "$cbo4", ComboIndex = 4, Width = 28,
            HorizontalAlign = ColumnAlign.Left, WrapText = true, Required = true },
        new() { Header = "Cán bộ phối hợp", Description = "Chọn từ danh sách", Placeholder = "$cbo6", ComboIndex = 6, Width = 32,
            HorizontalAlign = ColumnAlign.Left, WrapText = true, Required = true },
        new() { Header = "Ngày bắt đầu", Description = "dd/MM/yyyy", NumberFormat = "dd/MM/yyyy", Width = 16,
            HorizontalAlign = ColumnAlign.Center },
        new() { Header = "Ngày kết thúc", Description = "dd/MM/yyyy", NumberFormat = "dd/MM/yyyy", Width = 16,
            HorizontalAlign = ColumnAlign.Center },
        new() { Header = "Kinh phí", Description = "Số ≥ 0 (đồng)", NumberFormat = "#,##0", Width = 18,
            HorizontalAlign = ColumnAlign.Right },
        new() { Header = "Thời hạn hoàn thành", Description = "dd/MM/yyyy", NumberFormat = "dd/MM/yyyy", Width = 16,
            HorizontalAlign = ColumnAlign.Center },
    ];
}
