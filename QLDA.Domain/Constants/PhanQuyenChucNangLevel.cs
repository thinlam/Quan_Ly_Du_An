using System.ComponentModel;

namespace QLDA.Domain.Constants;


public enum PhanQuyenChucNangLevel
{
  
    [Description("Phòng ban")]
    PhongBan= 1,
    [Description("Người dùng chỉ định")]
    NguoiDung,
    [Description("Chức vụ")]
    ChucVu

}