using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.ToTrinhKeHoachs.DTOs;

public record ToTrinhKeHoachSearchDto : CommonSearchDto
{
    public string? SoQuyetDinh { get; set; }
}