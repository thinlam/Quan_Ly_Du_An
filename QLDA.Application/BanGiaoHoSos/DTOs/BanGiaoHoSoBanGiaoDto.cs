using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

/// <summary>
/// DTO cho endpoint ban-giao: đổi trạng thái 0→1, set ngày bàn giao, đính kèm biên bản
/// </summary>
public class BanGiaoHoSoBanGiaoDto {
    /// <summary>Ngày bàn giao (DateOnly). Server tự convert sang DateTimeOffset UTC via DateOnlyExtensions.</summary>
    public DateOnly? NgayBanGiao { get; set; }
    // Biên bản bàn giao (gắn khi thực hiện bàn giao)
    public List<TepDinhKemDto>? DanhSachBienBan { get; set; }
}
