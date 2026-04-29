namespace QLDA.Domain.Constants;

/// <summary>
/// Permission keys - match Ma values in DanhMucQuyen seed data
/// </summary>
public static class PermissionConstants {
    // Nhóm: DuAn
    public const string DuAn_XemTatCa = "DuAn.XemTatCa";
    public const string DuAn_XemTheoPhong = "DuAn.XemTheoPhong";
    public const string DuAn_Tao = "DuAn.Tao";
    public const string DuAn_Sua = "DuAn.Sua";
    public const string DuAn_Xoa = "DuAn.Xoa";
    public const string DuAn_PheDuyet = "DuAn.PheDuyet";

    // Nhóm: GoiThau
    public const string GoiThau_XemTatCa = "GoiThau.XemTatCa";
    public const string GoiThau_XemTheoPhong = "GoiThau.XemTheoPhong";
    public const string GoiThau_Tao = "GoiThau.Tao";
    public const string GoiThau_Sua = "GoiThau.Sua";
    public const string GoiThau_Xoa = "GoiThau.Xoa";

    // Nhóm: HopDong
    public const string HopDong_XemTatCa = "HopDong.XemTatCa";
    public const string HopDong_XemTheoPhong = "HopDong.XemTheoPhong";
    public const string HopDong_Tao = "HopDong.Tao";
    public const string HopDong_Sua = "HopDong.Sua";
    public const string HopDong_Xoa = "HopDong.Xoa";

    // Nhóm: VanBan
    public const string VanBan_XemTatCa = "VanBan.XemTatCa";
    public const string VanBan_XemTheoPhong = "VanBan.XemTheoPhong";
    public const string VanBan_Tao = "VanBan.Tao";
    public const string VanBan_Sua = "VanBan.Sua";
    public const string VanBan_Xoa = "VanBan.Xoa";

    // Nhóm: ThanhToan
    public const string ThanhToan_QuanLy = "ThanhToan.QuanLy";

    /// <summary>
    /// All permission keys grouped by NhomQuyen
    /// </summary>
    public static readonly Dictionary<string, string[]> ByNhom = new() {
        ["DuAn"] = [DuAn_XemTatCa, DuAn_XemTheoPhong, DuAn_Tao, DuAn_Sua, DuAn_Xoa, DuAn_PheDuyet],
        ["GoiThau"] = [GoiThau_XemTatCa, GoiThau_XemTheoPhong, GoiThau_Tao, GoiThau_Sua, GoiThau_Xoa],
        ["HopDong"] = [HopDong_XemTatCa, HopDong_XemTheoPhong, HopDong_Tao, HopDong_Sua, HopDong_Xoa],
        ["VanBan"] = [VanBan_XemTatCa, VanBan_XemTheoPhong, VanBan_Tao, VanBan_Sua, VanBan_Xoa],
        ["ThanhToan"] = [ThanhToan_QuanLy],
    };

    /// <summary>
    /// Get all XemTatCa permission keys
    /// </summary>
    public static string[] AllXemTatCa =>
        [DuAn_XemTatCa, GoiThau_XemTatCa, HopDong_XemTatCa, VanBan_XemTatCa];

    /// <summary>
    /// Get all XemTheoPhong permission keys
    /// </summary>
    public static string[] AllXemTheoPhong =>
        [DuAn_XemTheoPhong, GoiThau_XemTheoPhong, HopDong_XemTheoPhong, VanBan_XemTheoPhong];

    /// <summary>
    /// All Tao + Sua permission keys across modules
    /// </summary>
    private static readonly string[] AllTaoSua =
        [DuAn_Tao, DuAn_Sua, GoiThau_Tao, GoiThau_Sua, HopDong_Tao, HopDong_Sua, VanBan_Tao, VanBan_Sua];

    /// <summary>
    /// All permission keys in the system
    /// </summary>
    private static readonly string[] AllPermissions =
        [.. ByNhom.Values.SelectMany(p => p)];

    /// <summary>
    /// Default role → permission mapping (used for seed data + reference)
    /// Matches seed data in CauHinhVaiTroQuyenConfiguration
    /// </summary>
    public static readonly Dictionary<string, string[]> RolePermissions = new() {
        // Admin / Quản trị → all permissions
        [RoleConstants.QLDA_TatCa] = AllPermissions,
        [RoleConstants.QLDA_QuanTri] = AllPermissions,

        // Lãnh đạo → xem tất cả mọi module
        [RoleConstants.QLDA_LDDV] = AllXemTatCa,
        [RoleConstants.QLDA_LD] = AllXemTatCa,

        // Chuyên viên → xem theo phòng + tạo/sửa
        [RoleConstants.QLDA_ChuyenVien] =
            [.. AllXemTheoPhong, .. AllTaoSua],
    };
}
