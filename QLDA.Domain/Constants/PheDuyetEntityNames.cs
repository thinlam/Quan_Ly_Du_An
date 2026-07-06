using System.ComponentModel;
using System.Reflection;

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

    [ExcludeFromTypeList]
    [Description("Đề xuất nhu cầu kinh phí")]
    public const string DeXuatNhuCauKinhPhi = "DeXuatNhuCauKinhPhi";

    [ExcludeFromTypeList]
    [Description("Đề xuất kế hoạch kinh phí năm")]
    public const string DeXuatNhuCauKinhPhiNam = "DeXuatNhuCauKinhPhiNam";
   
    [ExcludeFromTypeList]
    [Description("Đề xuất")]
    public const string DeXuatMacDinhStt = "DeXuatMacDinh";// đối tượng dùng chung 
 
    [Description("Tờ trình có thẩm định")]
    public const string ToTrinhCoThamDinh = "ToTrinhCoThamDinh"; // đối tượng dùng chung 

    /// <summary>
    /// UC55 - Báo cáo kết quả khảo sát (#9474)
    /// </summary>
    [Description("Báo cáo kết quả khảo sát")]
    public const string BaoCaoKetQuaKhaoSat = "BaoCaoKetQuaKhaoSat";
    
    [Description("Tờ trình phê duyệt khảo sát")]
    public const string PheDuyetKhaoSat = "PheDuyetKhaoSat";

    [ExcludeFromTypeList]
    [Description("Thuyết minh dự án")]
    public const string ThuyetMinhDuAn = "ThuyetMinhDuAn";

    [Description("Thẩm định")]
    public const string ThamDinh = "ThamDinh";

    [Description("Tờ trình kết quả gói thầu")]
    public const string ToTrinhKetQuaGoiThau = "ToTrinhKetQuaGoiThau";

    [Description("Tờ trình thẩm định nhà thầu")]
    public const string ToTrinhThamDinhNhaThau = "ToTrinhThamDinhNhaThau";

    [Description("Tờ trình triển khai kế hoạch LCNT")]
    public const string TrienKhaiKeHoachLCNT = "TrienKhaiKeHoachLCNT";

    [Description("Tờ trình kế hoạch triển khai các hạng mục dự án")]
    public const string KeHoachTrienKhaiHangMuc = "KeHoachTrienKhaiHangMuc";

    [Description("Quyết định phê duyệt kế hoạch thuê")]
    public const string QuyetDinhKeHoachThue = "QuyetDinhKeHoachThue"; 

    [Description("Dự toán chuẩn bị đầu tư ")]
    public const string DuToanDauTu = "DuToanDauTu";
    [Description("Chủ trương lập kế hoạch")]
    public const string ChuTruongLapKeHoach = "ChuTruongLapKeHoach";
    [Description("Tờ trình không duyệt")]
    public const string ToTrinhKhongDuyet = "ToTrinhKhongDuyet";

    [Description("Kế hoạch lcnt dự toán hoặc sẵn có")]
    public const string KHLCNTDuToanSanCo = "KHLCNTDuToanSanCo";

    [Description("KHLCNT dự toán/KHTDV yêu cầu riêng")]
    public const string KHLCNTDuToanYeuCauRieng = "KHLCNTDuToanYeuCauRieng";
    
    [Description("Kế hoạch tổng thể lựa chọn nhà thầu")]
        public const string KeHoachTongTheLCNT = "KeHoachTongTheLCNT";
    [Description("Kế hoạch LCNT giai đoạn CBĐT")]
    public const string KeHoachLCNTChuanBiDauTu = "KeHoachLCNTChuanBiDauTu";
    [Description("Thỏa thuận giao việc")]
    public const string ThoaThuanGiaoViec = "ThoaThuanGiaoViec";
    [Description("Kế hoạch lcnt rút gọn")]
    public const string KeHoachLuaChonNhaThauRutGon = "KeHoachLuaChonNhaThauRutGon";
    [Description("Quyết định duyệt dự toán")] //issue #9478
    public const string QuyetDinhDuyetDuToan = "QuyetDinhDuyetDuToan";
    [Description("Quyết định lập ban quản lý dự án ")] //issue #9636
    public const string QuyetDinhLapBanQLDA = "QuyetDinhLapBanQLDA";

    /// <summary>
    /// UC63 — Thanh lý hợp đồng (Nghiệm thu thanh lý) (#9644)
    /// </summary>
    [Description("Thanh lý hợp đồng")]
    public const string ThanhLyHopDong = "ThanhLyHopDong";


}
public static class LoaiToTrinhKhongDuyetExtensions
{
    /// <summary>
    /// Kiểm tra xem một chuỗi có khớp với Description nào trong Enum không
    /// </summary>
    public static bool ContainsDescription(string? loai)
    {
        if (string.IsNullOrWhiteSpace(loai)) return false;

        // Duyệt qua tất cả các phần tử của Enum và đọc Description của từng thằng
        foreach (LoaiToTrinhKhongDuyetEnum enumValue in Enum.GetValues(typeof(LoaiToTrinhKhongDuyetEnum)))
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();

            // So sánh chuỗi truyền vào với Description (không phân biệt hoa thường)
            if (attribute != null && string.Equals(attribute.Description, loai, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
// Mục đích để loại trừ các loại ko hiển thị trong quản lý phê duyệt api/phe-duyet/types
[AttributeUsage(AttributeTargets.Field)]
public class ExcludeFromTypeListAttribute : Attribute
{
}