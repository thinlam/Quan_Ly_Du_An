using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class KeHoachTrienKhaiHangMucImportDescriptor : IImportDescriptor {
    public string EntityName => "Import kế hoạch triển khai hạng mục";
    public string TemplateFileName => "Import_KeHoachTrienKhaiHangMuc.xlsx";
    public string TableName => "KeHoachTrienKhaiHangMucImport";
    public string OutputPath { get; set; } = "";
    public string? Title => "MẪU IMPORT KẾ HOẠCH TRIỂN KHAI HẠNG MỤC";
    public string? HintText =>
        "Nhập dữ liệu vào bảng bên dưới. Giai đoạn / Cán bộ chọn từ danh sách. Ngày nhập dd/MM/yyyy.";

    public List<ImportColumn> Columns { get; } =
    [
        new() { Header = "Tên hạng mục", Description = "Bắt buộc", Width = 40,
            HorizontalAlign = ColumnAlign.Left, WrapText = true, Required = true },
        new() { Header = "Giai đoạn", Description = "Chọn từ danh mục", Placeholder = "$cbo1", ComboIndex = 1, Width = 22,
            HorizontalAlign = ColumnAlign.Left, Required = true },
        new() { Header = "Cán bộ chủ trì", Description = "Chọn từ danh sách đơn vị", Placeholder = "$cbo2", ComboIndex = 2, Width = 28,
            HorizontalAlign = ColumnAlign.Left, WrapText = true, Required = true },
        new() { Header = "Cán bộ phối hợp", Description = "Tùy chọn — cùng danh sách đơn vị", Placeholder = "$cbo3", ComboIndex = 3, Width = 28,
            HorizontalAlign = ColumnAlign.Left, WrapText = true, Required = true},
        new() { Header = "Ngày bắt đầu", Description = "dd/MM/yyyy", NumberFormat = "dd/MM/yyyy", Width = 16,
            HorizontalAlign = ColumnAlign.Center },
        new() { Header = "Ngày kết thúc", Description = "dd/MM/yyyy", NumberFormat = "dd/MM/yyyy", Width = 16,
            HorizontalAlign = ColumnAlign.Center },
        new() { Header = "Kinh phí", Description = "Số ≥ 0 (đồng)", NumberFormat = "#,##0", Width = 18,
            HorizontalAlign = ColumnAlign.Right },
        new() { Header = "Thời hạn hoàn thành", Description = "dd/MM/yyyy", NumberFormat = "dd/MM/yyyy", Width = 16,
            HorizontalAlign = ColumnAlign.Center },
        new() { Header = "Tờ trình", Description = "Bắt buộc", Width = 18,
            HorizontalAlign = ColumnAlign.Left, Required = true },
        new() { Header = "Ngày trình", Description = "dd/MM/yyyy", NumberFormat = "dd/MM/yyyy", Width = 16,
            HorizontalAlign = ColumnAlign.Center },
        new() { Header = "Trích yếu", Description = "Tùy chọn", Width = 35,
            HorizontalAlign = ColumnAlign.Left, WrapText = true },
    ];
}
