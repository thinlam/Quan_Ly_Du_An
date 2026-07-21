namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>


public class ToTrinhQuyetDinh : IAggregateRoot, IHasKey<long>
{

    public long Id { get; set; }
    public Guid? HoSoMoiThauToTrinhId { get; set; }

    public Guid? HoSoMoiThauQuyetDinhId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public int? ChucVu { get; set; }

}
