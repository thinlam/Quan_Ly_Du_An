using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class DeXuatChuyenTiep : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }

    public long? SoLieuGiaiNgan { get; set; }
    public long? UocGiaiNgan { get; set; }
    public long? NhuCauKinhPhi { get; set; }

    public string? KhoiLuongThucTe { get; set; }
    public string? KhoiLuongDuKien { get; set; }

    public int? TrangThaiId { get; set; }


    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}