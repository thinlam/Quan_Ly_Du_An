namespace QLDA.Domain.Constants;

/// <summary>
/// GroupType constants for QLDA module
/// Used by TepDinhKem and Attachment entities
/// </summary>
public static class GroupTypeConstants
{
    public const string GoiThau = "GoiThau";
    public const string KetQuaTrungThau = "KetQuaTrungThau";
    public const string KeHoachLuaChonNhaThau = "KeHoachLuaChonNhaThau";
    public const string NghiemThu = "NghiemThu";
    public const string TamUng = "TamUng";
    public const string ThanhToan = "ThanhToan";
    public const string KySo = "KySo";

    /// <summary>
    /// Bản lịch sử ký số trên TepDinhKem (đã bị thay thế; bản hiện hành dùng <see cref="KySo"/>).
    /// </summary>
    public const string NoiDungDaKySo = "NoiDungDaKySo";
    public const string PhanKhaiKinhPhi = "PhanKhaiKinhPhi";
    public const string ToTrinhKeHoach = "ToTrinhKeHoach";
    public const string ChuTruongMoi = "DeXuatChuTruongMoi";
    public const string ChuTruongChuyenTiep = "DeXuatChuyenTiep";
    public const string NhuCauKinhPhi = "DeXuatNhuCauKinhPhi";
}