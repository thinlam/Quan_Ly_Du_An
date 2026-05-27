using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Hồ sơ đề xuất cấp độ CNTT
/// </summary>
public class ThuyetMinhDuAn : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int?  BuocId { get; set; }
    public int? TrangThaiId { get; set; }
    public int? TrangThaiThamDinhId { get; set; }
    public DateTimeOffset NgayTrinh { get; set; }
    public string So { get; set; }
    public string? TrichYeu { get; set; }
    public string? KetQuaThamDinh { get; set; }


    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThaiThamDinh { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion


}