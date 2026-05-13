using System.ComponentModel.DataAnnotations;

namespace QLDA.Domain.Enums;

/// <summary>
/// Trạng thái bàn giao hồ sơ
/// </summary>
public enum ETrangThaiBanGiao {
    /// <summary>
    /// Khởi tạo - chưa bàn giao
    /// </summary>
    [Display(Name = "Khởi tạo")]
    KhoiTao = 1,

    /// <summary>
    /// Đã bàn giao cho phòng HC-TH
    /// </summary>
    [Display(Name = "Đã bàn giao")]
    DaBanGiao = 2
}
