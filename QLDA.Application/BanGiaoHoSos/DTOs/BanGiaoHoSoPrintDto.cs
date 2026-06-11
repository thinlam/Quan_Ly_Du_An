namespace QLDA.Application.BanGiaoHoSos.DTOs;

/// <summary>
/// DTO cho in Biên Bản Bàn Giao Hồ Sơ – flat, không nested collections
/// </summary>
public record BanGiaoHoSoPrintDto {
    public string? Ma { get; init; }
    public string? TenHoSo { get; init; }
    public string? TenDuAn { get; init; }
    public string? MaDuAn { get; init; }
    public string? TenPhongBanChuTri { get; init; }
    public string? TenPhongBanNhan { get; init; }
    public string? GhiChu { get; init; }
    public DateTimeOffset? NgayBanGiao { get; init; }
    public int TongSoTepDinhKem { get; init; }
}
