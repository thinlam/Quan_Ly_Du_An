using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Kế hoạch lựa chọn nhà thầu
/// </summary>
public class KeHoachLuaChonNhaThauRutGon
{

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? Ten { get; set; }
    public int? TrangThaiId { get; set; }
   
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }

    public Guid? GoiThauId { get; set; }
    public int? DonViTuVanId { get; set; }
    public int? KetQuaDanhGia { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public GoiThau? GoiThau { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }


    #endregion
}