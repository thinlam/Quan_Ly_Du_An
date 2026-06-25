using QLDA.Gen.Metadata;

namespace QLDA.Gen.Descriptors;

public class TinhHinhThucHienDauThauExportDescriptor : IExportDescriptor
{
    public string EntityName => "Báo cáo tình hình thực hiện đấu thầu";
    public string TemplateFileName => "TinhHinhThucHienDauThau.xlsx";
    public string OutputPath { get; set; } = string.Empty;
    public TemplateLayoutType Layout => TemplateLayoutType.LetterheadExport;
    public string? Title => "BÁO CÁO TÌNH HÌNH THỰC HIỆN ĐẤU THẦU";

    public List<ExportColumn> Columns { get; } =
    [
        new("Stt", "STT", 6, null, false, ColumnAlign.Center),
        new("TenDuAn", "Dự án", 28, null, true, ColumnAlign.Left),
        new("TenBuoc", "Bước", 40, null, true, ColumnAlign.Left),
        new("TenGoiThau", "Tên gói thầu", 35, null, true, ColumnAlign.Left),
        new("GiaGoiThau", "Giá gói thầu", 18, "#,##0", false, ColumnAlign.Right),
        new("TenNguonVon", "Nguồn vốn", 28, null, true, ColumnAlign.Left),
        new("TenHinhThucLuaChonNhaThau", "Hình thức lựa chọn nhà thầu", 30, null, true, ColumnAlign.Left),
        new("TenPhuongThucLuaChonNhaThau", "Phương thức lựa chọn nhà thầu", 30, null, true, ColumnAlign.Left),
        new("ThoiGianToChucLuaChonNhaThau", "Thời gian tổ chức lựa chọn nhà thầu", 22, null, false, ColumnAlign.Center),
        new("TenLoaiHopDong", "Loại hợp đồng", 24, null, false, ColumnAlign.Left),
    ];
}
