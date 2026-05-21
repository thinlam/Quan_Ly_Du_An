using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Hồ sơ đề xuất cấp độ CNTT
/// </summary>
public class HoSoDeXuatCapDoCntt : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int?  BuocId { get; set; }
    public int? TrangThaiId { get; set; }
    public int? CapDoId { get; set; }
    public DateTimeOffset? NgayTrinh { get; set; }

    public int? DonViChuTriId { get; set; }

    public string? NoiDungDeNghi { get; set; }

    public string? NoiDungBaoCao { get; set; }

    public string? NoiDungDuThao { get; set; }


    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DmCapDoCntt? CapDo { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion


}