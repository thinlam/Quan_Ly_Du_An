using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class ToTrinhThamDinhNhaThau : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string So { get; set; }

    public DateTimeOffset? NgayTrinh { get; set; }

    public string? TrichYeu { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public int? TrangThaiThamDinhId { get; set; }
    public int? TrangThaiId { get; set; } //Trạng thái phê duyệt

    public ICollection<NhaThauTrinhThamDinh>? NhaThaus { get; set; } = [];
    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}

public class NhaThauTrinhThamDinh :  IAggregateRoot
{
    public Guid ToTrinhId { get; set; }
    public int NhaThauId { get; set; }
    public string? TenNhaThau { get; set; }
    public string? KetQuaDanhGia { get; set; }
    public List<TepDinhKem>? DanhSachTepDinhKem { get; set; }

    #region Navigation Properties

    public ToTrinhThamDinhNhaThau? ToTrinhThamDinhNhaThau { get; set; }

    #endregion
}
