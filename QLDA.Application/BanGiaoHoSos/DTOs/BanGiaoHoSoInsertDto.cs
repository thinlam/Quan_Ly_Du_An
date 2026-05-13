using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

public class BanGiaoHoSoInsertDto : IMayHaveTepDinhKemDto {
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GhiChu { get; set; }
    // ⚠️ PhongBanChuTriId KHÔNG khai báo ở đây – tự động set từ _userProvider.Info.PhongBanID ?? _userProvider.Info.DonViID trong InsertCommand
    // Tệp HS bàn giao (gắn khi insert/update)
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
