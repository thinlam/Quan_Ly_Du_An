namespace QLDA.Application.Providers;

/// <summary>
/// Provider for QLDA application configuration values
/// </summary>
public interface IAppSettingsProvider {
    /// <summary>
    /// ID phòng kế toán - đơn vị có quyền CRUD ThanhToan
    /// </summary>
    long PhongKeToanID { get; }

    /// <summary>
    /// ID phòng Hành chính - Tổng hợp
    /// </summary>
    long PhongHCTHID { get; }
}