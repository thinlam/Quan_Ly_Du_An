namespace QLDA.Application.Providers;

/// <summary>
/// Provider for QLDA application configuration values
/// </summary>
public interface IAppSettingsProvider {
    /// <summary>
    /// ID Phòng Kế Hoạch - Tài chính - đơn vị có quyền CRUD ThanhToan
    /// </summary>
    long PhongKHTCId { get; }

    /// <summary>
    /// ID Phòng Hành chính - Tổng hợp
    /// </summary>
    long PhongHCTHId { get; }

    /// <summary>
    /// ID phòng Kế hoạch - Tài chính. Nhân viên thuộc phòng này có global bypass cho authorization.
    /// </summary>
    long PhongKHTCID { get; }
}