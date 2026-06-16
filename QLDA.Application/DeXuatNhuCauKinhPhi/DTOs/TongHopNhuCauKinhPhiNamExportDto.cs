namespace QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;

/// <summary>
/// Dòng export Excel tổng hợp nhu cầu kinh phí năm — property khớp placeholder template ($Field)
/// </summary>
public class TongHopNhuCauKinhPhiNamExportDto {
    public int Stt { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public string? PhongPctTrinh { get; set; }
    public string? PhongKhtcTongHop { get; set; }
    public string? PhongBgdPheDuyet { get; set; }
}
