using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

/// <summary>
/// Kết quả LCNT
/// </summary>
public class KetQuaTrungThau : Entity<Guid>, IAggregateRoot, ITienDo, IQuyetDinh {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid GoiThauId { get; set; }
    public long GiaTriTrungThau { get; set; }
    public Guid? DonViTrungThauId { get; set; }
    public long? SoNgayTrienKhai { get; set; }
    public string? TrichYeu { get; set; }
    public int? LoaiGoiThauId { get; set; }
    public DateTimeOffset? NgayEHSMT { get; set; }
    public DateTimeOffset? NgayMoThau { get; set; }
    public long? SoNgayThucHienHopDong { get; set; }

    #region Issue 9208
    /// <summary>
    /// Số quyết định
    /// </summary>
    public string? SoQuyetDinh { get; set; }
    /// <summary>
    /// Ngày quyết định
    /// </summary>
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    #endregion

    #region Navigation Properties

    public GoiThau? GoiThau { get; set; }
    public DuAn? DuAn { get; set; }
    public DuAnBuoc? DuAnBuoc { get; set; }
    public DanhMucNhaThau? DonViTrungThau { get; set; }
    public DanhMucLoaiGoiThau? LoaiGoiThau { get; set; }
    #endregion
}