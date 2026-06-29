using BuildingBlocks.Application.Common.DTOs;

namespace QLDA.Application.DuAns.DTOs;

public class TheoDoiDuAnTheoGiaiDoanResultDto
{
    public int TongSoDuAn { get; set; }
    public int ConHan { get; set; }
    public int QuaHan { get; set; }
    public int DaHoanThanh { get; set; }

    public PaginatedList<TheoDoiDuAnTheoGiaiDoanDto> DanhSach { get; set; } = new();
}
