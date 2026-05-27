using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.ToTrinhPheDuyets.DTOs;

public record ToTrinhPheDuyetSearchDto : CommonSearchDto
{
    public string? SoQuyetDinh { get; set; }
    public string? Loai { get; set; }
}