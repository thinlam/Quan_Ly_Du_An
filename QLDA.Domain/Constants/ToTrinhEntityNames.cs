using System.ComponentModel;
using System.Reflection;

namespace QLDA.Domain.Constants;

/// <summary>
/// Entity name constants for polymorphic PheDuyetHistory
/// </summary>
public static class ToTrinhEntityNames
{

    /// <summary>
    /// 9621 - 
    /// </summary>
    #region task 9621

    [Description("KHLCNT dự toán/KHTDV sẵn có")]
    public const string KHLCNTDuToanSanCo = "KHLCNTDuToanSanCo";

    [Description("KHLCNT dự toán/KHTDV yêu cầu riêng")]
    public const string KHLCNTDuToanYeuCauRieng = "KHLCNTDuToanYeuCauRieng";

    [Description("Kế hoạch tổng thể lựa chọn nhà thầu")]
    public const string KeHoachTongTheLCNT = "KeHoachTongTheLCNT";

    [Description("Kế hoạch LCNT giai đoạn CBĐT")]
    public const string KeHoachLCNTChuanBiDauTu = "KeHoachLCNTChuanBiDauTu";
   
    #endregion

    [Description("Quyết định kế hoạch thuê")]
    public const string QuyetDinhKeHoachThue = "QuyetDinhKeHoachThue";
    [Description("Tờ trình kế hoạch ")]
    public const string ToTrinhKeHoach = "ToTrinhKeHoach";
    [Description("Phê duyệt khảo sát")]
    public const string PheDuyetKhaoSat = "PheDuyetKhaoSat";
  
  
}
// chú giải: Các tờ trình ko cần duyệt( chỉ trình là hoàn thành quá trình phê duyệt)
public enum LoaiToTrinhKhongDuyetEnum
{
    [Description("KHLCNTDuToanSanCo")]
    KHLCNTDuToanSanCo,

    [Description("KHLCNTDuToanYeuCauRieng")]
    KHLCNTDuToanYeuCauRieng,

    [Description("KeHoachTongTheLCNT")]
    KeHoachTongTheLCNT,

    [Description("KeHoachLCNTChuanBiDauTu")]
    KeHoachLCNTChuanBiDauTu
}


public static class ToTrinhEntityNamesExtensions
{
    // Tạo bộ nhớ đệm (Cache) chứa tất cả các giá trị chuỗi hợp lệ
    private static readonly HashSet<string> ValidEntityNames = typeof(ToTrinhEntityNames)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Select(f => f.GetRawConstantValue() as string)
        .Where(val => val != null)
        .Select(val => val!)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Kiểm tra xem loại tờ trình truyền vào có nằm trong danh sách cấu hình hay không
    /// </summary>
    public static bool ContainsEntity(string loai)
    {
        if (string.IsNullOrEmpty(loai)) return false;

        // Trả về true/false ngay lập tức không cần loop hay dùng reflection lại nữa
        return ValidEntityNames.Contains(loai);
    }
}


