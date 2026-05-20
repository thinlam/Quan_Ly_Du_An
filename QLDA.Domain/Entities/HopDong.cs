using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng hợp đồng
/// </summary>
public class HopDong : Entity<Guid>, IAggregateRoot, ITienDo {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid GoiThauId { get; set; }
    public string? Ten { get; set; }
    public string? SoHopDong { get; set; }
    public string? NoiDung { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public long? GiaTri { get; set; }
    public DateTimeOffset? NgayHieuLuc { get; set; }

    /// <summary>
    /// Ngày dự kiến kết thúc hợp đồng = Ngày hiệu lực + Thời gian thực hiện hợp đồng (từ Kết quả trúng thầu)
    /// </summary>
    public DateTimeOffset? NgayDuKienKetThucHopDong { get; set; }

    /// <summary>
    /// Ngày dự kiến kết thúc gói thầu = Ngày hiệu lực + Thời gian thực hiện gói thầu (từ Kết quả trúng thầu)
    /// </summary>
    public DateTimeOffset? NgayDuKienKetThucGoiThau { get; set; }

    public int? LoaiHopDongId { get; set; }

    /// <summary>
    /// Đơn vị thực hiện
    /// </summary>
    public Guid? DonViThucHienId { get; set; }

    /// <summary>
    /// Là biên bản giao nhiệm vụ hay là hợp đồng
    /// </summary>
    public bool IsBienBan { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DuAnBuoc? DuAnBuoc { get; set; }
    public DanhMucNhaThau? DonViThucHien { get; set; }
    public GoiThau? GoiThau { get; set; }
    public DanhMucLoaiHopDong? LoaiHopDong { get; set; }
    public ICollection<PhuLucHopDong>? PhuLucHopDongs { get; set; }
    public ICollection<NghiemThu>? NghiemThus { get; set; }
    /// <summary>
    /// Tạm ứng chỉ được 1 lần
    /// </summary>
    public TamUng? TamUng { get; set; }

    #endregion
}