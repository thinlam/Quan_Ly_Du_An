using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class GoiThau : Entity<Guid>, IAggregateRoot, ITienDo {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    /// <summary>
    /// Kế hoạch lựa chọn nhà thầu
    /// </summary>
    public Guid? KeHoachLuaChonNhaThauId { get; set; }
    public string? Ten { get; set; }
    public long? GiaTri { get; set; }
    public int? LoaiHopDongId { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }
    public int? PhuongThucLuaChonNhaThauId { get; set; }
    public string? ThoiGianLuaNhaThau { get; set; }
    public string? ThoiGianHopDong { get; set; }
    public int? NguonVonId { get; set; }
    public string? TomTatCongViecChinhGoiThau { get; set; }
    public string? ThoiGianBatDauToChucLuaChonNhaThau { get; set; }
    public int? ThoiGianThucHienGoiThau { get; set; }
    public string? TuyChonMuaThem { get; set; }
    public string? GiamSatHoatDongDauThau { get; set; }

    /// <summary>
    /// Gói thầu đã được duyệt
    /// </summary>
    public bool DaDuyet { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DuAnBuoc? DuAnBuoc { get; set; }
    public KeHoachLuaChonNhaThau? KeHoachLuaChonNhaThau { get; set; }
    public DanhMucHinhThucLuaChonNhaThau? HinhThucLuaChonNhaThau { get; set; }
    public DanhMucPhuongThucLuaChonNhaThau? PhuongThucLuaChonNhaThau { get; set; }
    public DanhMucLoaiHopDong? LoaiHopDong { get; set; }
    public DanhMucNguonVon? NguonVon { get; set; }
    public HopDong? HopDong { get; set; }
    public KetQuaTrungThau? KetQuaTrungThau { get; set; }
    #endregion
}