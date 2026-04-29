
using BuildingBlocks.Domain.Attributes;

namespace BuildingBlocks.Domain.DTOs;

public class UserInfo {
    /// <summary>
    /// UserMaster.UserPortalId
    /// </summary>
    public long UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    /// <summary>
    /// UserMaster.DonViId or DmDonVi.DonViId
    /// </summary>
    public long? DonViID { get; set; }
    /// <summary>
    /// UserMaster.PhongBanId or DmDonVi.DonViId
    /// </summary>
    public long? PhongBanID { get; set; }

    [IgnoreClaim]
    public string TenDonVi { get; set; } = string.Empty;
    [IgnoreClaim]
    public string TenPhongBan { get; set; } = string.Empty;
    [IgnoreClaim]
    public int TinhThanhID { get; set; }
    [IgnoreClaim]
    public int QuanHuyenID { get; set; }
    [IgnoreClaim]
    public int PhuongXaID { get; set; }
    [IgnoreClaim]
    public string MaDonViID { get; set; } = string.Empty;
    [IgnoreClaim]
    public int CapDonViID { get; set; }
    [IgnoreClaim]
    public long CanBoID { get; set; }
    [IgnoreClaim]
    public bool LaDonViChinh { get; set; }
    [IgnoreClaim]
    public bool Used { get; set; }

    public UserInfo() {
    }
    public UserInfo(string userId, string donViId, string phongBanId) {
        UserID = long.TryParse(userId, out var uid) ? uid : -1;
        DonViID = long.TryParse(donViId, out var did) ? did : -1;
        PhongBanID = long.TryParse(phongBanId, out var pid) ? pid : -1;
    }
}
