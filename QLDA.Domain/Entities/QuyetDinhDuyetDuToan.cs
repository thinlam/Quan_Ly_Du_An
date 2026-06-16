using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

/// <summary>
/// Quyết định duyệt quyết toán
/// </summary>
public class QuyetDinhDuyetDuToan : Entity<Guid>, IAggregateRoot, ITienDo, IApprovableEntity
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? TrangThaiId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }
    public decimal? GiaTri { get; set; }
    public int? HinhThucQuanLyId { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public Guid? KeHoachLuaChonNhaThauId { get; set; }
    public ICollection<QuyetDinhDuyetDuToanChiPhi>? ChiPhis { get; set; } = [];
    public ICollection<QuyetDinhDuyetDuToanNguonVon>? KeHoachVons { get; set; } = [];
    #region Navigation Properties

    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    public DuAn? DuAn { get; set; }
    public DuAnBuoc? DuAnBuoc { get; set; }
    public KeHoachLuaChonNhaThau? KeHoachLuaChonNhaThau { get; set; }
    public DanhMucHinhThucQuanLy? HinhThucQuanLyDuAn { get; set; }

    #endregion
}

