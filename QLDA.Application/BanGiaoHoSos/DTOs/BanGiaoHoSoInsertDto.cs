using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

public class BanGiaoHoSoInsertDto : IMayHaveTepDinhKemDto {
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public long? PhongBanChuTriId { get; set; }
    // Tệp HS bàn giao (gắn khi insert/update)
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
