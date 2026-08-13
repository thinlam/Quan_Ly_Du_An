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
    /// <summary>
    /// Biên bản thương thảo của Kết quả trúng thầu (Issue #169)
    /// </summary>
    KetQuaTrungThau_BienBanThuongThao,
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
    QuyetDinhPheDuyetNhiemVu,
    HoSoDeXuatCapDoCntt,
    /// <summary>
    /// Tệp hồ sơ bàn giao
    /// </summary>
    BanGiaoHoSo,
    /// <summary>
    /// Biên bản bàn giao
    /// </summary>
    BienBanBanGiao,
    HoSoMoiThauDienTu,
    HoSoMoiThauDienTuToTrinh,
    HoSoMoiThauDienTuQuyetDinh,
    HoSoMoiThauDienTuQuyetDinhTD,
    HoSoMoiThauDienTuCamKetTD,
    HoSoMoiThauDienTuBaoCaoTD,
    PhanKhaiKinhPhi,
    ToTrinhKeHoach,
    DeXuatChuTruongMoi,
    DeXuatChuyenTiep,
    DeXuatNhuCauKinhPhi,
    DeXuatNhuCauKinhPhiNam,
    ThuyetMinhDuAn,
    ThuyetMinhDuAnThamDinh,
    ToTrinhThamDinhNhaThau,
    NoiDungToTrinhThamDinhNhaThau,
    KetQuaThamDinhNhaThau,
    TrienKhaiKeHoachLCNT,
    DonViTuVan,
    KeHoachTrienKhai,
    ChuTruongLapKeHoach,
    ThoaThuanGiaoViec,
    KeHoachLuaChonNhaThauRutGon,
    DuToanDauTu,
    /// <summary>
    /// File Khác của Dự toán CBĐT / Trình duyệt dự toán (Issue #175)
    /// </summary>
    DuToanDauTu_Khac,
    ThanhLyHopDong,
    ThanhLyHopDong_BienBanNghiemThu,
    ThanhLyHopDong_Khac,
    BaoCaoKetQuaKhaoSat,
    QuyetDinhDieuChinh,
    ToTrinhPheDuyet,
    PheDuyetKetQuaGoiThauDuAn,
    KeHoachTrienKhaiHangMuc,
    ToTrinhQuyetDinh,
    KeHoachTrienKhaiChiTietDuAn,
    QuyetDinhKeHoachThue,
    QuyetDinhKeHoachThueThamDinh,
    QuyetDinhDuyetDuToan_Khac,
    NoiDungThamDinhNhaThau,
    QuyetDinhDuyetDuToan,
    ToTrinhCoThamDinh,

    #region Issue #179 — Tờ trình thẩm định nhà thầu (đối chiếu/thương thảo/thẩm định/quyết định)
    /// <summary>
    /// File E-HSDT của ThongTinNhaThau
    /// </summary>
    ToTrinhThamDinhNhaThau_FileEHSDT,
    /// <summary>
    /// File đánh giá của ThongTinNhaThau
    /// </summary>
    ToTrinhThamDinhNhaThau_FileDanhGia,
    /// <summary>
    /// File của bước Đối chiếu
    /// </summary>
    ToTrinhThamDinhNhaThau_DoiChieu,
    /// <summary>
    /// File của bước Thương thảo
    /// </summary>
    ToTrinhThamDinhNhaThau_ThuongThao,
    /// <summary>
    /// File của bước Thẩm định
    /// </summary>
    ToTrinhThamDinhNhaThau_ThamDinh,
    /// <summary>
    /// File của QuyetDinhPheDuyet (VanBanQuyetDinh)
    /// </summary>
    ToTrinhThamDinhNhaThau_QuyetDinh,
    #endregion
}
