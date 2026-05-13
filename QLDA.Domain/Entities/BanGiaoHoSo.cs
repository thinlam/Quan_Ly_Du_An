using QLDA.Domain.Enums;
using QLDA.Domain.Entities.ViMaster;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng quản lý bàn giao hồ sơ từ người dùng → Phòng HC-TH
/// </summary>
public class BanGiaoHoSo : Entity<Guid>, IAggregateRoot {
    /// <summary>
    /// Mã bản giao hồ sơ
    /// </summary>
    public string? Ma { get; set; }

    /// <summary>
    /// Tên hồ sơ
    /// </summary>
    public string? TenHoSo { get; set; }

    /// <summary>
    /// FK → DuAn
    /// </summary>
    public Guid? DuAnId { get; set; }

    /// <summary>
    /// FK → DanhMucBuoc
    /// </summary>
    public int? BuocId { get; set; }

    /// <summary>
    /// FK → Phòng ban chủ trì (phòng HC-TH hoặc tương tự)
    /// </summary>
    public long? PhongBanChuTriId { get; set; }

    /// <summary>
    /// FK → UserMaster (người tạo hồ sơ - từ Auth)
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? GhiChu { get; set; }

    /// <summary>
    /// Trạng thái: 0 = Khởi tạo, 1 = Đã bàn giao
    /// </summary>
    public ETrangThaiBanGiao TrangThai { get; set; } = ETrangThaiBanGiao.KhoiTao;

    /// <summary>
    /// Ngày bàn giao – set khi gọi endpoint ban-giao
    /// </summary>
    public DateTimeOffset? NgayBanGiao { get; set; }

    #region Navigation Properties
    public UserMaster? User { get; set; }
    public DanhMucDonVi? PhongBanChuTri { get; set; }
    public DuAn? DuAn { get; set; }
    public DanhMucBuoc? Buoc { get; set; }
    #endregion
}
