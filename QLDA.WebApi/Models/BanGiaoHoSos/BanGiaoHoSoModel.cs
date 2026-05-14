using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.BanGiaoHoSos;

// Response model cho endpoint chi-tiet – không implement request interfaces
public class BanGiaoHoSoModel {
    public Guid Id { get; set; }
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public long? PhongBanChuTriId { get; set; }
    public long? PhongBanNhanId { get; set; }
    public string? TenPhongBanNhan { get; set; }
    public int TrangThai { get; set; }  // 0: Khởi tạo, 1: Đã bàn giao
    public DateOnly? NgayBanGiao { get; set; }  // Entity lưu DateTimeOffset, convert khi map
    public string? GhiChu { get; set; }
    // Tệp HS bàn giao (EGroupType.BanGiaoHoSo)
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
    // Biên bản bàn giao (EGroupType.BienBanBanGiao)
    public List<TepDinhKemModel>? DanhSachBienBanBanGiao { get; set; }
}
