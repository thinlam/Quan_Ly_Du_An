namespace QLDA.WebApi.ConfigurationOptions;

public class AppSettings {
    public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    public string AllowedHosts { get; set; } = string.Empty;
    public JwtSettings Jwt { get; set; } = new JwtSettings();
    public string? SwaggerPathBase { get; set; }
    /// <summary>
    /// ID Phòng Kế Hoạch - Tài chính - đơn vị có quyền CRUD ThanhToan
    /// </summary>
    public long PhongKHTCId { get; set; }

    /// <summary>
    /// ID Phòng Hành chính - Tổng hợp
    /// </summary>
    public long PhongHCTHId { get; set; }

    /// <summary>
    /// ID phòng Kế hoạch - Tài chính. Nhân viên thuộc phòng này có global bypass cho authorization.
    /// </summary>
    public long PhongKHTCID { get; set; }
}