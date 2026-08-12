using QLDA.Domain.Enums;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dùng chung cho 3 bước xử lý của "Tờ trình thẩm định nhà thầu" (Issue #179):
/// Đối chiếu / Thương thảo / Thẩm định — phân biệt bằng <see cref="Loai"/>
/// (<see cref="ELoaiBuocXuLyThamDinhNhaThau"/>), không tạo 3 bảng riêng.
/// </summary>
public class ToTrinhThamDinhBuocXuLy : IAggregateRoot, IHasKey<long>
{
    public long Id { get; set; }
    public Guid ToTrinhId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    /// <summary>
    /// Nullable — mục Đối chiếu cho phép để trống trên UI.
    /// </summary>
    public string? NoiDung { get; set; }

    /// <summary>
    /// DoiChieu / ThuongThao / ThamDinh — xem <see cref="ELoaiBuocXuLyThamDinhNhaThau"/>.
    /// </summary>
    public int Loai { get; set; }

    #region Navigation Properties
    public ToTrinhThamDinhNhaThau? ToTrinh { get; set; }
    #endregion
}
