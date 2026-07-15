namespace QLDA.Application.DuAns.DTOs;

public class TheoDoiDuAnPhongPhanCongResultDto
{
    public int TongSoDuAn { get; set; }
    public int ConHan { get; set; }
    public int QuaHan { get; set; }
    public int DaHoanThanh { get; set; }

    public PaginatedList<TheoDoiDuAnPhongPhanCongDto> DanhSach { get; set; } = new();
}
