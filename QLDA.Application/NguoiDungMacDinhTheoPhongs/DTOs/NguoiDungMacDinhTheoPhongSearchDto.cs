using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

public record NguoiDungMacDinhTheoPhongSearchDto : CommonSearchDto
{
    public long? PhongBanId { get; init; }
    public long? NguoiDungId { get; init; }
    public string? Keyword { get; init; }
}
