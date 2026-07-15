using QLDA.Domain.Constants;

namespace QLDA.Domain.Entities;

/// <summary>
/// Cấu hình vai trò quyền - Role-Permission toggle table
/// Maps roles to permissions with active/inactive toggle
/// </summary>
public class PhanQuyenChucNang : Entity<int>, IAggregateRoot {

    /// Bật/tắt quyền cho vai trò
    public bool SuDung { get; set; }
    public string MaChucNang { get; set; } = string.Empty;
    public string ChucNang { get; set; } = string.Empty;
    public PhanQuyenChucNangLevel? Level { get; set; }   // NguoiDungMacDinhID, NguoiDungChiDinh, TheoChucVu
    public ICollection<PhanQuyenChucNangCapDo>? DanhSachChiTiet { get; set; } = [];

}
public class PhanQuyenChucNangCapDo {
    public int QuyenId { get; set; }
    public long LevelId { get; set; }//PhongBanId,ChucVuid,User_porttalId
    public bool? NguoiDungMacDinh { get; set; } // nếu là phòng ban thì có thẻ chọn ng dùng măc định
    public List<long>? NguoiDungChiDinhs { get; set; } // nếu là phòng ban thì có thẻ chọn ng dùng măc định
    public PhanQuyenChucNang? Quyen { get; set; }

}
/*
 * ví dụ chức năng them moi du an
 * Phuong thức : Phòng ban
 * Danh sách chi tiết : 
 * 1/ Phòng 1 : Mặc định -> load user mặc định của phòng
 * 2/ Phòng Giám đốc -> All
 * Ví dụ 2 : Chức Năng Thanh Toán
 * Phuog thức : Phòng ban
 * Danh sách chi tiết : Phòng Kế hoạch -> ko chọn ( load ALl)
 */

