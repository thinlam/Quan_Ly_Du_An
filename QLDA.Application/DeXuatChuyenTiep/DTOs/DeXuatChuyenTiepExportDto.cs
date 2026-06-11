namespace QLDA.Application.DeXuatChuyenTieps.DTOs;

/// <summary>
/// Dòng dữ liệu export Excel đề xuất chủ trương chuyển tiếp — tên property khớp placeholder template ($Field)
/// </summary>
public class DeXuatChuyenTiepExportDto {
    public int Stt { get; set; }
    public long? SoLieuGiaiNgan { get; set; }
    public long? UocGiaiNgan { get; set; }
    public long? NhuCauKinhPhi { get; set; }
    public string? KhoiLuongThucTe { get; set; }
    public string? KhoiLuongDuKien { get; set; }
}
