using System.ComponentModel;

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
   
    [Description("DuToanDauTu")]
    public const string DuToanDauTu = "DuToanDauTu";

  
}
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



