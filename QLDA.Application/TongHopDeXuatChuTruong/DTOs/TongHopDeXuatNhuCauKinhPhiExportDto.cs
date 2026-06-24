namespace QLDA.Application.TongHopDeXuatChuTruongs.DTOs;

/// <summary>
/// Dòng dữ liệu export Excel đề xuất nhu cầu kinh phí chủ trương — property khớp placeholder template ($Field).
/// </summary>
public class TongHopDeXuatNhuCauKinhPhiExportDto {
    public int Stt { get; set; }
    public string? TrichYeu { get; set; }
    public long? KinhPhiDeXuat { get; set; }
    public string? TenPhongDeXuat { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public string? TenTrangThai { get; set; }
}
