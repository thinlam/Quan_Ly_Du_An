using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class TinhHinhThucHienDauThauExportDescriptor : IExportDescriptor
{
    public string EntityName => "Báo cáo tình hình thực hiện đấu thầu";
    public string TemplateFileName => "TinhHinhThucHienDauThau.xlsx";
    public string OutputPath { get; set; } = string.Empty;
    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;
    public string? Title => "BÁO CÁO TÌNH HÌNH THỰC HIỆN ĐẤU THẦU";
    public bool HandMaintainedTemplate => true; // Template được giữ nguyên bởi người dùng.

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT"),
        new("TenDuAn", "Dự án"),
        new("TenBuoc", "Bước"),
        new("TenGoiThau", "Tên gói thầu"),
        new("GiaGoiThau", "Giá gói thầu"),
        new("TenNguonVon", "Nguồn vốn"),
        new("TenHinhThucLuaChonNhaThau", "Hình thức lựa chọn nhà thầu"),
        new("TenPhuongThucLuaChonNhaThau", "Phương thức lựa chọn nhà thầu"),
        new("ThoiGianToChucLuaChonNhaThau", "Thời gian tổ chức lựa chọn nhà thầu"),
        new("TenLoaiHopDong", "Loại hợp đồng"),
    ];
}
