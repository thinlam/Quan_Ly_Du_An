using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Interfaces;
using System.ComponentModel;

namespace QLDA.Domain.Entities;

[DisplayName("Tờ trình thẩm định nhà thầu")]
public class ToTrinhThamDinhNhaThau : Entity<Guid>, IAggregateRoot, ITienDo
{
    public new Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string So { get; set; } = string.Empty;
    public DateTimeOffset NgayTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public int? TrangThaiId { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public bool? DaThamDinh { get; set; }
    public List<KetQuaThamDinhNhaThau>? NhaThaus { get; set; } = [];

    #region Issue #179 — Tờ trình thẩm định nhà thầu (1 gói thầu / 1 nhà thầu)
    /// <summary>
    /// Gói thầu — chỉ lưu Id, GiaTri/HinhThucLCNT luôn load lại từ <see cref="GoiThau"/>, không lưu duplicate.
    /// </summary>
    public Guid? GoiThauId { get; set; }
    public string? TenNhaThau { get; set; }
    public DateTimeOffset? NgayKetThucDanhGia { get; set; }
    public List<ToTrinhThamDinhBuocXuLy>? BuocXuLys { get; set; } = [];
    #endregion

    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    public GoiThau? GoiThau { get; set; }
    #endregion


}