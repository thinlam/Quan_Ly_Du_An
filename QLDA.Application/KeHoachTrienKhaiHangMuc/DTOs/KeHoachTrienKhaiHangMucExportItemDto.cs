using System.Text.Json.Serialization;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

/// <summary>
/// Một dòng export — group header (A/B/…) hoặc hạng mục công việc.
/// Property khớp placeholder template ($Stt, $GiaiDoan, …).
/// </summary>
public class KeHoachTrienKhaiHangMucExportItemDto
{
    public string? Stt { get; set; }
    public string? GiaiDoan { get; set; }
    public string? TenHangMuc { get; set; }
    public string? DonViChuTri { get; set; }
    public string? DonViPhoiHop { get; set; }
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public int? ThoiHan { get; set; }
    public string? CanBoChuTri { get; set; }
    public string? CanBoPhoiHop { get; set; }
    public long? KinhPhi { get; set; }

    /// <summary>Dòng header giai đoạn (A/B/…) — dùng post-style export, không map ra Excel.</summary>
    [JsonIgnore]
    public bool IsGroupHeader { get; set; }
}
