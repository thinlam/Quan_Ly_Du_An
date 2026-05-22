using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class DeXuatNhuCauKinhPhi : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public long? KinhPhiDeXuat { get; set; }
    public long? DonViDeXuatId { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public DateTimeOffset? NgayPhieuChuyen { get; set; }
    public string? TrichYeu { get; set; }
    
    public int? TrangThaiId { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}