using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ThanhToans.DTOs;

public class ThanhToanUpdateDto : IMayHaveTepDinhKemInsertOrUpdateDto {
    public Guid Id { get; set; }
    public Guid NghiemThuId { get; set; }
    public string? SoHoaDon { get; set; }
    public DateTimeOffset? NgayHoaDon { get; set; }
    public long GiaTri { get; set; }
    public string? NoiDung { get; set; }
    public List<Guid>? PhuLucs { get; set; }

    public List<TepDinhKemInsertOrUpdateDto>? DanhSachTepDinhKem { get; set; }
}
public class phuLucHopDongCboDto
{
    public Guid phuLucHopDongId { get; set; }
    public String Ten { get; set; }

}