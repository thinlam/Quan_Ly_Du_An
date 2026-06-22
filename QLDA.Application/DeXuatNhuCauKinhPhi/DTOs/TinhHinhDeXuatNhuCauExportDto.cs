namespace QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;

/// <summary>
/// Dòng export Excel tình hình đề xuất nhu cầu — property khớp placeholder template ($Field)
/// </summary>
public class TinhHinhDeXuatNhuCauExportDto {
    public int Stt { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public string? PhongPctTrinh { get; set; }
    public string? PhongKhtcTongHop { get; set; }
    public string? PhongBgdPheDuyet { get; set; }
}
