using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Kế hoạch lựa chọn nhà thầu
/// </summary>
public class ThoaThuanGiaoViec : Entity<Guid>, IAggregateRoot
{

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? TrangThaiId { get; set; }
   
    public Guid? GoiThauId { get; set; }
    public string? PhamVi { get; set; }
    public string? NoiDung { get; set; }
    public int? ThoiGian { get; set; }
    public long? GiaTri { get; set; }
    public string? ChatLuong { get; set; }


    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public GoiThau? GoiThau { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }

    #endregion
}