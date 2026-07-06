using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class ThanhLyHopDong : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }
    public Guid? HopDongId { get; set; }
    public int? TrangThaiId { get; set; }


    #region Navigation Properties

    public ICollection<ThanhLyHopDongNghiemThu>? DanhSachNghiemThus { get; set; } = [];
    public HopDong? HopDong { get; set; }
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}