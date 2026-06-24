using System.ComponentModel;

namespace QLDA.Domain.Constants;


public enum DuongDiToTrinhRoleLevel
{
    [Description("Bất kỳ")]
    BatKy = 0,
    [Description("Phòng ban chủ trì")]
    PhongBanChuTri = 1,

    [Description("Người dùng chỉ định")]
    NguoiDung,

    [Description("Phòng ban chỉ định")]
    PhongBanChiDinh,
    [Description("Người phụ trách chính")]
    NguoiPhuTrachChinh,
    [Description("Đơn vị chỉ định")]
    DonVi
}