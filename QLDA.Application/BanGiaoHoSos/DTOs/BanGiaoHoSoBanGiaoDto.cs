using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

/// <summary>
/// DTO cho endpoint ban-giao: đổi trạng thái 0→1, set ngày bàn giao, đính kèm biên bản
/// </summary>
public class BanGiaoHoSoBanGiaoDto {
    /// <summary>Ngày bàn giao, mặc định là DateTime.Now nếu null</summary>
    public DateTimeOffset? NgayBanGiao { get; set; }
    // Biên bản bàn giao (gắn khi thực hiện bàn giao)
    public List<TepDinhKemDto>? DanhSachBienBan { get; set; }
}
