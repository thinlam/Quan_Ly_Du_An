using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

public class VanBanQuyetDinh : Entity<Guid>, IAggregateRoot, ITienDo, IVanBanQuyetDinh, INguoiKy, IEntityType {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? Loai { get; set; }

    #region Issue #179
    /// <summary>
    /// Trạng thái phê duyệt của chính bản ghi Quyết định — nullable để không ảnh hưởng nghiệp vụ cũ
    /// (dữ liệu cũ NULL mặc định hiểu là ĐÃ DUYỆT). Chỉ nghiệp vụ mới (VD: Tờ trình thẩm định nhà thầu)
    /// mới set giá trị này (Chờ duyệt → Đã duyệt).
    /// Đặt tên khác <c>TrangThaiId</c> vì tên này đã được 1 số bảng con TPT
    /// (<see cref="PheDuyetDuToan"/>, <see cref="QuyetDinhLapBanQLDA"/>) khai báo riêng cho mục đích khác;
    /// không thể trùng tên property trong cùng cây kế thừa EF Core (TPT).
    /// </summary>
    public int? TrangThaiDuyetId { get; set; }
    /// <summary>
    /// Chức vụ người ký — đặt tên khác <c>ChucVuId</c> vì tên này đã được 1 số bảng con TPT
    /// (<see cref="PheDuyetDuToan"/>, <see cref="VanBanPhapLy"/>, <see cref="VanBanChuTruong"/>)
    /// khai báo riêng; không thể trùng tên property trong cùng cây kế thừa EF Core (TPT).
    /// </summary>
    public int? NguoiKyChucVuId { get; set; }
    #endregion

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DuAnBuoc? DuAnBuoc { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThaiDuyet { get; set; }
    public DanhMucChucVu? NguoiKyChucVu { get; set; }
    #endregion
}