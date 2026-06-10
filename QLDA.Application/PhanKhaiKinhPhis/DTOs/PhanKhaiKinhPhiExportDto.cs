namespace QLDA.Application.PhanKhaiKinhPhis.DTOs;

/// <summary>
/// Dòng dữ liệu export Excel kết quả phân khai vốn đã duyệt — tên property khớp placeholder template ($Field)
/// </summary>
public class PhanKhaiKinhPhiExportDto {
    public int Stt { get; set; }
    public string? SoToTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public string? TenNguonVon { get; set; }
    public decimal? KinhPhiDeXuat { get; set; }
    public decimal? KinhPhiPhanKhai { get; set; }
    public string? TenTrangThai { get; set; }
}
