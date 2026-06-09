using System.ComponentModel;

namespace QLDA.Domain.Constants;

/// <summary>
/// </summary>
public static class LoaiDeXuatLCNTonstants
{
    public static class Default
    {
        public const string XinChuTruong = "Lập kế hoạch lcnt";
        public const string KhongLap = "Không lập kế hoạch lcnt";
    }

   
    public enum LoaiDeXuatMacDinh
    {
        XinChuTruong = 1,
        KhongLap = 2
    }
    public enum ELoaiDeXuatKeHoachLCNT
    {
        [Description(Default.XinChuTruong)] XinChuTruong = (int)LoaiDeXuatMacDinh.XinChuTruong,
        [Description(Default.KhongLap)]     KhongLap = (int)LoaiDeXuatMacDinh.KhongLap,
    }
}
