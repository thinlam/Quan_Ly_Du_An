using QLDA.Domain.Enums;

namespace QLDA.Application.Common.Constants;
/// <summary>
/// Sử dụng khi muốn trả về constant partial view từ E_ManHinh(database) hay DanhMucManHinh(Class)
/// </summary>
public static class LoaiVanBanQuyetDinhConst {
    // Constants
    
    /// <summary>
    /// Quyết định duyệt dự án
    /// </summary>
    private const string QuyetDinhDuyetDuAn = "QUYETDINHDUYETQUYETTOAN";

    /// <summary>
    /// Quyết định duyệt KHLCNT
    /// </summary>
    public const string QuyetDinhDuyetKHLCNT = "QUYETDINHDUYETKHLCNT";
    /// <summary>
    /// Quyết định duyệt quyết toán
    /// </summary>
    private const string QuyetDinhDuyetQuyetToan = "QUYETDINHDUYETQUYETTOAN";

    /// <summary>
    /// Quyết định lập Ban QLDA
    /// </summary>
    public const string QuyetDinhLapBanQLDA = "QUYETDINHTLBANQLDUAN";

    /// <summary>
    /// Quyết định lập Bên mời thầu
    /// </summary>
    private const string QuyetDinhLapBenMoiThau = "QUYETDINHTLBENMOITHAU";


    /// <summary>
    /// Quyết định lập Hội đồng thẩm định
    /// </summary>
    private const string QuyetDinhLapHoiDongThamDinh = "QUYETDINHLAPHOIDONGTHAMDINH";

    /// <summary>
    /// Văn bản pháp lý
    /// </summary>
    private const string VanBanPhapLy = "VANBANPHAPLY";

    /// <summary>
    /// Văn bản chủ trương
    /// </summary>
    private const string VanBanChuTruong = "VANBANCHUTRUONG";

    /// <summary>
    /// Kế hoạch lựa chọn nhà thầu
    /// </summary>
    private const string KeHoachLuaChonNhaThau = "QUYETDINHKEHOACH";

    /// <summary>
    /// Phê duyệt dự toán
    /// </summary>
    private const string PheDuyetDuToan = "PHEDUYETDUTOAN";
    // Dictionary constant (readonly)
    public static readonly Dictionary<string, string> Dictionary = new() {
        { nameof(EnumLoaiVanBanQuyetDinh.QuyetDinhDuyetDuAn), QuyetDinhDuyetDuAn },
        { nameof(EnumLoaiVanBanQuyetDinh.QuyetDinhDuyetKHLCNT), QuyetDinhDuyetKHLCNT },
        { nameof(EnumLoaiVanBanQuyetDinh.QuyetDinhDuyetQuyetToan), QuyetDinhDuyetQuyetToan },
        { nameof(EnumLoaiVanBanQuyetDinh.QuyetDinhLapBanQLDA), QuyetDinhLapBanQLDA },
        { nameof(EnumLoaiVanBanQuyetDinh.QuyetDinhLapBenMoiThau), QuyetDinhLapBenMoiThau },
        { nameof(EnumLoaiVanBanQuyetDinh.QuyetDinhLapHoiDongThamDinh), QuyetDinhLapHoiDongThamDinh },
        { nameof(EnumLoaiVanBanQuyetDinh.VanBanPhapLy), VanBanPhapLy },
        { nameof(EnumLoaiVanBanQuyetDinh.VanBanChuTruong), VanBanChuTruong },
        { nameof(EnumLoaiVanBanQuyetDinh.KeHoachLuaChonNhaThau), KeHoachLuaChonNhaThau },
        { nameof(EnumLoaiVanBanQuyetDinh.PheDuyetDuToan), PheDuyetDuToan },
    };
}