namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;

/// <summary>
/// Dòng export Excel tổng hợp nhu cầu kinh phí năm — property khớp placeholder template ($Field)
/// </summary>
public class TongHopNhuCauKinhPhiNamExportDto {
    public int Stt { get; set; }
    public string? SoKeHoach { get; set; }
    public string? TrichYeu { get; set; }
    public long? TongHopChiPhi { get; set; }
    public string? Ngay { get; set; }
    public string? TrangThai { get; set; }
    public int? SoLuongTepDinhKem { get; set; }
}
