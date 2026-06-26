namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

public record NguoiDungMacDinhTheoPhongCreateDto
{
    public long PhongBanId { get; init; }
    public long NguoiDungId { get; init; }
}
