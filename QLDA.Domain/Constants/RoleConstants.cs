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
    /// được xác định qua IAppSettingsProvider.PhongHCTHId (User.PhongBanID),
    /// không dùng role riêng.
    /// </summary>
    public const string QLDA_LDDV = "QLDA_LDDV";
    /// <summary>
    /// NVTT - Bộ phận 01: được phép xem toàn bộ DuAn và Bước (read-only).
    /// Không cấp quyền write/edit. Áp dụng cả cho child entities (HopDong,
    /// GoiThau, VanBan...) thông qua DuAnId-based filter.
    /// </summary>
    public const string NVTT_BP01 = "NVTT_BP01";
    /// <summary>
    /// Role dùng chung cho user NVTT (Trưởng phòng xử lý, Trưởng phòng phối hợp,
    /// Giám đốc, ...) cần xem DuAn/Buoc QLDA. Read-all + CUD khi được assign DuAn
    /// (qua ownership filter: Lãnh đạo phụ trách / Phòng phụ trách chính / Phối hợp).
    /// </summary>
    public const string NVTT_XemDuAn = "NVTT_XemDuAn";
    /// <summary>
    /// Nhóm role có quyền xem tất cả DuAn và Bước.
    /// Đăng ký role mới tại đây để cấp quyền read-all mà không cần sửa provider.
    /// Write path (CanExecuteAsync, CanExecuteStepAsync) LUÔN fallback về ownership
    /// check — user có role trong group này vẫn CUD được DuAn được assign.
    /// </summary>
    public const string GroupReadAll = $"{NVTT_BP01},{NVTT_XemDuAn}";
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