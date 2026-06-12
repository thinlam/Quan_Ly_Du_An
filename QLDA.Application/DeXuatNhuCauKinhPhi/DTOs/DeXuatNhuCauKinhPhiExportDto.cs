namespace QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;

/// <summary>
/// Dòng dữ liệu export Excel danh mục xin chủ trương đầu tư — tên property khớp placeholder template ($Field)
/// </summary>
public class DeXuatNhuCauKinhPhiExportDto {
    public int Stt { get; set; }
    public string? TrichYeu { get; set; }
    public long? KinhPhiDeXuat { get; set; }
    public string? TenPhongDeXuat { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public string? TenTrangThai { get; set; }
}
