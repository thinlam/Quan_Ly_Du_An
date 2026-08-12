namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dùng chung cho các "Tờ trình/Quyết định" dạng So/Ngay/NguoiKy/ChucVu/TrichYeu.
/// Dùng <see cref="EntityId"/> + <see cref="Loai"/> (<see cref="Enums.ELoaiToTrinhQuyetDinh"/>)
/// để phân biệt nghiệp vụ sở hữu thay vì mỗi nghiệp vụ 1 FK riêng (Issue #179).
/// </summary>
public class ToTrinhQuyetDinh : IAggregateRoot, IHasKey<long>
{
    public long Id { get; set; }

    /// <summary>
    /// Id của entity nghiệp vụ đang sở hữu dòng này (VD: HoSoMoiThauDienTu.Id, ToTrinhThamDinhNhaThau.Id).
    /// </summary>
    public Guid? EntityId { get; set; }

    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public int? ChucVu { get; set; }

    /// <summary>
    /// Xác định nghiệp vụ sở hữu — xem <see cref="Enums.ELoaiToTrinhQuyetDinh"/>.
    /// </summary>
    public int Loai { get; set; }
}
