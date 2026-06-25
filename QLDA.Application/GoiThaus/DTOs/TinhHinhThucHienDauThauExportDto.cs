namespace QLDA.Application.GoiThaus.DTOs;

/// <summary>
/// Dòng export báo cáo tình hình thực hiện đấu thầu — property khớp placeholder template ($Field)
/// </summary>
public class TinhHinhThucHienDauThauExportDto
{
    public int Stt { get; set; }
    public string? TenDuAn { get; set; }
    public string? TenBuoc { get; set; }
    public string? TenGoiThau { get; set; }
    public long? GiaGoiThau { get; set; }
    public string? TenNguonVon { get; set; }
    public string? TenHinhThucLuaChonNhaThau { get; set; }
    public string? TenPhuongThucLuaChonNhaThau { get; set; }
    public string? ThoiGianToChucLuaChonNhaThau { get; set; }
    public string? TenLoaiHopDong { get; set; }
}
