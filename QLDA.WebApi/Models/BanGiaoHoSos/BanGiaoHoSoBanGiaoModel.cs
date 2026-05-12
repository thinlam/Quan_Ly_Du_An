using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.BanGiaoHoSos;

/// <summary>
/// Model cho endpoint PUT /ban-giao: nhận ngày bàn giao + biên bản bàn giao
/// </summary>
public class BanGiaoHoSoBanGiaoModel {
    /// <summary>Ngày bàn giao (DateOnly), nếu null sẽ dùng ngày hiện tại. Server tự quy đổi sang UTC.</summary>
    public DateOnly? NgayBanGiao { get; set; }
    // Biên bản bàn giao (đính kèm khi thực hiện bàn giao)
    public List<TepDinhKemModel>? DanhSachBienBan { get; set; }
}
