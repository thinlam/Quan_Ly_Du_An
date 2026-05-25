using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// UC55 — Báo cáo kết quả khảo sát, nghiệm thu khảo sát
/// </summary>
public class BaoCaoKetQuaKhaoSat : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }

    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungNghiemThu { get; set; }
    public DateTimeOffset? NgayKhaoSat { get; set; }
    public int? TrangThaiId { get; set; }
    public DateTimeOffset? NgayTrinh { get; set; }

    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}
