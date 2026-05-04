namespace QLDA.Domain.Enums;

/// <summary>
/// Loại đính kèm
/// </summary>
public enum EGroupType {
    /// <summary>
    /// File lỗi
    /// </summary>
    None = 0,
    VanBanPhapLy,
    VanBanChuTruong,
    PheDuyetDuToan,
    GoiThau,
    HopDong,
    KhoKhanVuongMac,
    KetQuaXuLyKhoKhanVuongMac,
    KetQuaTrungThau,
    BaoCaoTienDo,
    PhuLucHopDong,
    NghiemThu,
    ThanhToan,
    KeHoachLuaChonNhaThau,
    QuyetDinhDuyetDuAn,
    TamUng,
    QuyetDinhDuyetKHLCNT,
    QuyetDinhDuyetQuyetToan,
    QuyetDinhLapBanQLDA,
    QuyetDinhLapBenMoiThau,
    QuyetDinhLapHoiDongThamDinh,
    KySo,
    DangTaiKeHoachLcntLenMang,
    BaoCaoBaoHanhSanPham,
    BaoCaoBanGiaoSanPham,
    DuToan,
    /// <summary>
    /// Tệp đính kèm của Kế hoạch vốn
    /// </summary>
    KeHoachVon,
    /// <summary>
    /// Tệp quyết định phê duyệt nhiệm vụ và dự toán kinh phí của Dự án
    /// </summary>
    QuyetDinhPheDuyetNhiemVu


}