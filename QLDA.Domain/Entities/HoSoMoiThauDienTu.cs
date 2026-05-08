using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng quản lý hồ sơ mời thầu điện tử
/// </summary>
public class HoSoMoiThauDienTu : Entity<Guid>, IAggregateRoot {
    public Guid? DuAnId { get; set; }

    public int? BuocId { get; set; }

    public int? HinhThucLuaChonNhaThauId { get; set; }

    public Guid? GoiThauId { get; set; }

    public long? GiaTri { get; set; }

    public string? ThoiGianThucHien { get; set; }

    public bool TrangThaiDangTai { get; set; }

    public int? TrangThaiId { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    
    public DuAnBuoc? Buoc { get; set; }

    public DanhMucHinhThucLuaChonNhaThau? HinhThucLuaChonNhaThau { get; set; }
    public GoiThau? GoiThau { get; set; }

    public DanhMucTrangThaiPheDuyet? TrangThaiPheDuyet { get; set; }

    #endregion
}