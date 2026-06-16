using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using System.ComponentModel;

namespace QLDA.Domain.Entities;

public class DuongDiTrangThaiToTrinh :IHasKey<long>, IAggregateRoot
{
    public long Id { get; set; }
    public string Loai { get; set; } = string.Empty;
    public string? MaTrangThaiHienTai { get; set; }
    public string? MaTrangThaiTiepTheo { get; set; }
    public string? TenTrangThaiTiepTheo { get; set; }
     //   RoleLevel : 1 là Phòng ban Chủ trì, 2 là User chỉ định (UserPortalId), 3 Là Phòng Ban chỉ định, 4 là đơn vị chỉ định
    public long? RoleId { get; set; }
    public DuongDiToTrinhRoleLevel? RoleLevel { get; set; }
    public bool Used { get; set; }
    public bool? IsDeleted { get; set; }

}

public enum DuongDiToTrinhRoleLevel
{
    [Description("Bất kỳ")]
    BatKy =0,
    [Description("Phòng ban chủ trì")]
    PhongBanChuTri =1,

    [Description("Người dùng chỉ định")]
    NguoiDung,

    [Description("Phòng ban chỉ định")]
    PhongBanChiDinh,
    [Description("Người phụ trách chính")]
    NguoiPhuTrachChinh,
    [Description("Đơn vị chỉ định")]
    DonVi
}