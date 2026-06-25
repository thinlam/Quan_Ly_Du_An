namespace QLDA.Application.PhanKhaiKinhPhis.DTOs;

/// <summary>
/// Dòng export danh sách phân khai kinh phí — property khớp placeholder template ($Field)
/// </summary>
public class PhanKhaiKinhPhiDanhSachExportDto {
    public int Stt { get; set; }
    public string? TenDuAn { get; set; }
    public long? TongMucDauTu { get; set; }
    public string? SoToTrinh { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public decimal? KinhPhiDeXuat { get; set; }
    public decimal? KinhPhiPhanKhai { get; set; }
    public string? TenTrangThai { get; set; }
}
