using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>


public class KetQuaThamDinhNhaThau : Entity<Guid>
{
    public Guid ToTrinhId { get; set; }
    public Guid NhaThauId { get; set; }
    public string? KetQuaDanhGia { get; set; }
    //public List<TepDinhKem>? DanhSachTepDinhKem { get; set; }

    #region Navigation Properties

    public ToTrinhThamDinhNhaThau? ToTrinhThamDinhNhaThau { get; set; }
    public DanhMucNhaThau? NhaThau { get; set; }

    #endregion
}
