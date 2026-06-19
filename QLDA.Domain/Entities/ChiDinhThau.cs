using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>


public class ChiDinhThau : IAggregateRoot, IHasKey<long>
{

    public long Id { get; set; }
    public Guid HoSoMoiThauId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public int? ChucVu { get; set; }
    #region Navigation Properties
    public HoSoMoiThauDienTu? HoSoMoiThauDienTu { get; set; }
    #endregion
}
