namespace QLDA.WebApi.ConfigurationOptions;

public class AppSettings {
    public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    public string AllowedHosts { get; set; } = string.Empty;
    public JwtSettings Jwt { get; set; } = new JwtSettings();
    public string? SwaggerPathBase { get; set; }
    /// <summary>
    /// ID phòng kế toán - đơn vị có quyền CRUD ThanhToan
    /// </summary>
    public long PhongKeToanID { get; set; }

    /// <summary>
    /// ID phòng Hành chính - Tổng hợp
    /// </summary>
    public long PhongHCTHID { get; set; }
}