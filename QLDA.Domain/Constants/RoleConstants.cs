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
    /// Lãnh đạo đơn vị (BGĐ + Trưởng phòng). Phòng Hành chính - Tổng hợp
    /// được xác định qua IAppSettingsProvider.PhongHCTHID (User.PhongBanID),
    /// không dùng role riêng.
    /// </summary>
    public const string QLDA_LDDV = "QLDA_LDDV";
    /// <summary>
    /// Quyền Admin hoặc Manager
    /// </summary>
    public const string GroupAdminOrManager = $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV}";

    /// <summary>
    /// Kết xuất Excel kết quả phân khai vốn được duyệt (CB/LĐ.PCT, GĐ/PGĐ, CB/LĐ.PKH-TC)
    /// </summary>
    public const string GroupPhanKhaiKinhPhiExport = $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV},{QLDA_ChuyenVien}";

    /// <summary>
    /// Kết xuất Excel danh sách đề xuất chủ trương chuyển tiếp (CB/LĐ.PCT, GĐ/PGĐ, CB/LĐ.PKH-TC)
    /// </summary>
    public const string GroupDeXuatChuTruongChuyenTiepExport = $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV},{QLDA_ChuyenVien}";

    /// <summary>
    /// Kết xuất Excel danh mục xin chủ trương đầu tư (CB.PCT, LĐ.PCT)
    /// </summary>
    public const string GroupXinChuTruongDauTuExport =
        $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV},{QLDA_ChuyenVien}";

    /// <summary>
    /// Kết xuất Excel tổng hợp nhu cầu kinh phí năm (CB/LĐ.PCT, GĐ/PGĐ, CB/LĐ.PKH-TC)
    /// </summary>
    public const string GroupTongHopNhuCauKinhPhiNamExport =
        $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV},{QLDA_ChuyenVien}";

    /// <summary>
    /// Kết xuất Excel báo cáo đề xuất chủ trương (CB/LĐ.PCT, GĐ/PGĐ, CB/LĐ.PKH-TC)
    /// </summary>
    public const string GroupBaoCaoDeXuatChuTruongExport =
        $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV},{QLDA_ChuyenVien}";

}