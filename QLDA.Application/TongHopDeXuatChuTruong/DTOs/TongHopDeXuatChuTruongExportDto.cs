namespace QLDA.Application.TongHopDeXuatChuTruongs.DTOs;

/// <summary>
/// Dòng dữ liệu export Excel — tên property khớp placeholder template ($Field).
/// </summary>
public class TongHopDeXuatChuTruongExportDto {
    public int Stt { get; set; }
    public string LoaiDeXuat { get; set; } = string.Empty;
    public string TenDuAn { get; set; } = string.Empty;
    public string? PhongBanPhuTrach { get; set; }
}
