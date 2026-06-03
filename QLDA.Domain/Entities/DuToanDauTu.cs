using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

/// <summary>
/// Quyết định duyệt quyết toán
/// </summary>
public class DuToanDauTu : Entity<Guid>, IAggregateRoot, ITienDo
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? PhuongAnThietKeId { get; set; }
    public long? TongMucDauTu { get; set; }
    public long? TongDuToan { get; set; }
    public string? NoiDungChiPhis { get; set; }
    public int? NguonVonId { get; set; }
    public int? Nam { get; set; }
    public string? SoToTrinh { get; set; }
    public DateTimeOffset? NgayTrinh { get; set; }
    public string? TrichYeu { get; set; }

    public int? TrangThaiId { get; set; }

    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    public DanhMucNguonVon? NguonVon { get; set; }
    public DanhMucPhuongAnThietKe? PhuongAnThietKe { get; set; }
    #endregion
}