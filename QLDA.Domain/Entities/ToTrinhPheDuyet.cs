using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class ToTrinhPheDuyet : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string So { get; set; }

    public DateTimeOffset? NgayToTrinh { get; set; }

    public string? TrichYeu { get; set; }
    public string Loai { get; set; }
    public int? TrangThaiId { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}