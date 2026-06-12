using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class ToTrinhCoThamDinh : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string So { get; set; }

    public DateTimeOffset? NgayToTrinh { get; set; }

    public string? TrichYeu { get; set; }
    public string? Loai { get; set; } // hiện tại chỉ có 1 loại, nhưng để sau này mở rộng thêm loại khác nếu cần
    public int? TrangThaiId { get; set; }
    public int? TrangThaiThamTraId { get; set; }
    public string? KetQuaThamDinh { get; set; }
    public string? KetQuaThamTra { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}
public enum TrangThaiThamTra
{
    ChuaThamTra = 1,
    DaThamTra = 2
}
