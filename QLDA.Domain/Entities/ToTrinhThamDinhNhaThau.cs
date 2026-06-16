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

    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion


}