namespace QLDA.Application.TongHopVanBanQuyetDinhs.DTOs;

/// <summary>
/// Dòng export Excel tổng hợp văn bản quyết định — property khớp placeholder template ($Field)
/// </summary>
public class TongHopVanBanQuyetDinhExportDto
{
    public int Stt { get; set; }
    public string? TenDuAn { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? Loai { get; set; }
    public string? TrichYeu { get; set; }
}
