using SequentialGuid;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.BanGiaoHoSos;

public class BanGiaoHoSoModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel {
    public Guid? Id { get; set; }
    
    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public long? PhongBanChuTriId { get; set; }
    public int TrangThai { get; set; }  // 0: Khởi tạo, 1: Đã bàn giao
    public DateTimeOffset? NgayBanGiao { get; set; }
    public string? GhiChu { get; set; }
    // Tệp HS bàn giao (EGroupType.BanGiaoHoSo)
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
    // Biên bản bàn giao (EGroupType.BienBanBanGiao) – chỉ đọc khi hiển thị
    public List<TepDinhKemModel>? DanhSachBienBanBanGiao { get; set; }
}
