namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

public record NguoiDungMacDinhTheoPhongUpdateDto
{
    public Guid Id { get; init; }
    public long PhongBanId { get; init; }
    public long NguoiDungId { get; init; }
}
