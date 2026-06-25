namespace QLDA.Application.BanGiaoHoSos.DTOs;

/// <summary>
/// Dòng export danh sách bàn giao hồ sơ — property khớp placeholder template ($Field)
/// </summary>
public class BanGiaoHoSoDanhSachExportDto
{
    public int Stt { get; set; }
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public string? TenPhongBan { get; set; }
    public string? NgayTao { get; set; }
    public string? TenTrangThai { get; set; }
}
