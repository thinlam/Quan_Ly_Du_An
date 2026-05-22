using System.ComponentModel;

namespace QLDA.Domain.Constants;

/// <summary>
/// Entity name constants for polymorphic PheDuyetHistory
/// </summary>
public static class PheDuyetEntityNames
{

    public const string Default = "Default";
    /// <summary>
    /// 9583 - Phê duyệt dự toán
    /// </summary>
    [Description("Phê duyệt dự toán")]
    public const string PheDuyetDuToan = "PheDuyetDuToan";
    /// <summary>
    /// 9488 - Hồ sơ đề xuất cấp độ ATTT
    /// </summary>
    [Description("Hồ sơ đề xuất cấp CNTT")]
    public const string HoSoDeXuatCapDoCntt = "HoSoDeXuatCapDoCntt";
    /// <summary>
    /// 9473/9485 - Hồ sơ mời thầu điện tử
    /// </summary>
    [Description("Hồ sơ mời thầu điện tử")]
    public const string HoSoMoiThauDienTu = "HoSoMoiThauDienTu";
    /// <summary>
    /// 9467 - Phân khai kinh phí
    /// </summary>
    [Description("Phân khai kinh phí")]
    public const string PhanKhaiKinhPhi = "PhanKhaiKinhPhi";
    /// <summary>
    /// Quyết định điều chỉnh phê duyệt
    /// </summary>
    [Description("Quyết định điều chỉnh")]
    public const string QuyetDinhDieuChinh = "QuyetDinhDieuChinh";

    [Description("Tờ trình kế hoạch")]
    public const string ToTrinhKeHoach = "ToTrinhKeHoach";
   
    [Description("Đề xuất chủ trương mới")]
    public const string DeXuatChuTruongMoi = "DeXuatChuTruongMoi";
   
    [Description("Đề xuất chủ trương chuyển tiếp")]
    public const string DeXuatChuTruongChuyenTiep = "DeXuatChuyenTiep";

    [Description("Đề xuất chủ trương chuyển tiếp")]
    public const string DeXuatNhuCauKinhPhi = "DeXuatNhuCauKinhPhi";
    [Description("Đề xuất")]
    public const string DeXuatMacDinhStt = "DeXuatMacDinh";


}
