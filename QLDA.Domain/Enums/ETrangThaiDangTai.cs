using System.ComponentModel;

namespace QLDA.Domain.Enums;

/// <summary>
/// Trạng thái đăng tải — dùng cho KetQuaTrungThau (issue #169)
/// </summary>
public enum ETrangThaiDangTai {
    [Description("Đã đăng")] DaDang = 1,
    [Description("Chưa đăng")] ChuaDang = 2,
}
