using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.ToTrinhThamDinhNhaThaus;

/// <summary>
/// Dùng chung cho 3 bước xử lý (Đối chiếu/Thương thảo/Thẩm định) — Issue #179.
/// </summary>
public class ToTrinhThamDinhBuocXuLyModel
{
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? NoiDung { get; set; }
    public List<TepDinhKemModel>? File { get; set; }
}
