using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

/// <summary>
/// Thanh toán - THANHTOAN
/// </summary>
public class ThanhToan : Entity<Guid>, IAggregateRoot, ITienDo {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid NghiemThuId { get; set; }
    /// <summary>
    /// Số hóa đơn
    /// </summary>
    public string? SoHoaDon { get; set; }

    public DateTimeOffset? NgayHoaDon { get; set; }

    /// <summary>
    /// Giá trị thanh toán
    /// </summary>
    public long? GiaTri { get; set; }

    public string? NoiDung { get; set; }
    public List<Guid>? PhuLucHopDongIds { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DuAnBuoc? DuAnBuoc { get; set; }
    public NghiemThu? NghiemThu { get; set; }

    #endregion
}