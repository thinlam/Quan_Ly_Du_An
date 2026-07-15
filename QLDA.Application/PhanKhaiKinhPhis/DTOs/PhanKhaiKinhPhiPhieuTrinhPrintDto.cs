namespace QLDA.Application.PhanKhaiKinhPhis.DTOs;

public class PhanKhaiKinhPhiPhieuTrinhPrintDto {
    public string? SoToTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public string? TenDuAn { get; set; }
    public string? TenNguonVon { get; set; }
    public string? ThuyetMinh { get; set; }
    public decimal? KinhPhiDeXuat { get; set; }
    public decimal? KinhPhiPhanKhai { get; set; }
    public long? TongMucDauTu { get; set; }
}
