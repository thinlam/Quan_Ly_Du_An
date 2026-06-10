namespace QLDA.Domain.Constants;

public static class RoleConstants {
    /// <summary>
    /// Quyền Administrator - Toàn quyền trong hệ thống
    /// </summary>
    public const string QLDA_TatCa = "QLDA_TatCa";
    /// <summary>
    /// Quyền Quản trị hệ thống
    /// </summary>
    public const string QLDA_QuanTri = "QLDA_QuanTri";
    /// <summary>
    /// Quyền Nhân viên
    /// </summary>
    public const string QLDA_ChuyenVien = "QLDA_ChuyenVien";
    /// <summary>
    /// Lãnh đạo (LDDV - Lãnh đạo đơn vị)
    /// </summary>
    public const string QLDA_LDDV = "QLDA_LDDV";
    /// <summary>
    /// Phòng Hành chính - Tổng hợp
    /// </summary>
    public const string QLDA_HC_TH = "QLDA_HC_TH";
    /// <summary>
    /// Quyền Admin hoặc Manager
    /// </summary>
    public const string GroupAdminOrManager = $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV}";

    /// <summary>
    /// Kết xuất Excel kết quả phân khai vốn được duyệt (CB/LĐ.PCT, GĐ/PGĐ, CB/LĐ.PKH-TC)
    /// </summary>
    public const string GroupPhanKhaiKinhPhiExport = $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV},{QLDA_ChuyenVien}";
}