using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

public class BanGiaoHoSoDto {
    public Guid Id { get; set; }
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public Guid? DuAnId { get; set; }
    public string? TenDuAn { get; set; }
    public int? BuocId { get; set; }
    public string? TenBuoc { get; set; }
    public long? PhongBanChuTriId { get; set; }
    public string? TenPhongBan { get; set; }
    public string? TenNguoiTao { get; set; }
    public string? GhiChu { get; set; }
    public int TrangThai { get; set; }  // 0: Khởi tạo, 1: Đã bàn giao
    public string? TenTrangThai { get; set; }
    public DateOnly? NgayBanGiao { get; set; }  // Entity lưu DateTimeOffset, convert khi map
    public DateTimeOffset? CreatedAt { get; set; }
    // Tệp HS bàn giao (EGroupType.BanGiaoHoSo)
    public List<TepDinhKemDto>? DanhSachTepHSBanGiao { get; set; }
    // Biên bản bàn giao (EGroupType.BienBanBanGiao)
    public List<TepDinhKemDto>? DanhSachBienBanBanGiao { get; set; }
}
