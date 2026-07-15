using System.Text.Json.Serialization;

namespace QLDA.Application.KhoKhanVuongMacs.DTOs;

/// <summary>
/// Dòng export Excel khó khăn vướng mắc — property khớp placeholder template ($Field)
/// </summary>
public class KhoKhanVuongMacExportDto
{
    public int Stt { get; set; }
    public string? TenDuAn { get; set; }
    public string? TenBuoc { get; set; }

    [JsonPropertyName("ngay")]
    public DateTimeOffset? Ngay { get; set; }

    [JsonPropertyName("tinhTrangId")]
    public int? TinhTrangId { get; set; }

    [JsonPropertyName("noiDung")]
    public string? NoiDung { get; set; }

    [JsonPropertyName("mucDoKhoKhanId")]
    public int? MucDoKhoKhanId { get; set; }
}
