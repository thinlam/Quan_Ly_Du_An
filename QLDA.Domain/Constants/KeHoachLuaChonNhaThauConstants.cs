using System.ComponentModel;

namespace QLDA.Domain.Constants;

/// <summary>
/// GroupType constants for QLDA module
/// Used by TepDinhKem and Attachment entities
/// </summary>
public enum KeHoachLuaChonNhaThauLoai
{
    [Description("Kế hoạch LCNT")]
    KeHoachLCNT,

    [Description("Kế hoạch lcnt dự toán hoặc sẵn có")]
    KHLCNTDuToanSanCo,

    [Description("Kế hoạch LCNT dự toán/KHTDV yêu cầu riêng")]
    KHLCNTDuToanYeuCauRieng,

    [Description("Kế hoạch tổng thể lựa chọn nhà thầu")]
    KeHoachTongTheLCNT,

    [Description("Kế hoạch LCNT giai đoạn CBĐT")]
    KeHoachLCNTChuanBiDauTu
}
