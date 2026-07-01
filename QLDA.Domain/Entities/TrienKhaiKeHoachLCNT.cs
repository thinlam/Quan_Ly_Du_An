using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class TrienKhaiKeHoachLCNT : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid GoiThauId { get; set; }
    public string So { get; set; } = string.Empty;
    public DateTimeOffset NgayTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public int? HinhThucLCNT { get; set; }
    public int? TrangThaiId { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public long? GiaTri { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public string? NoiDung { get; set; }
    public string? YeuCau { get; set; }
    public List<DonViTuVanKeHoach>? DonViTuVans { get; set; }

    #region Navigation Properties
    public DanhMucHinhThucLuaChonNhaThau? DmHinhThucLCNT { get; set; }
    public DuAn? DuAn { get; set; }
    public GoiThau? GoiThau { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}