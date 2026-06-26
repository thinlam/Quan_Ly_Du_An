namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

public record NguoiDungMacDinhTheoPhongDto
{
    public Guid Id { get; init; }
    public long PhongBanId { get; init; }
    public string? TenPhongBan { get; init; }
    public long NguoiDungId { get; init; }
    public string? TenNguoiDung { get; init; }
}
