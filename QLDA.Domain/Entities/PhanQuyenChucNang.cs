using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Entities.ViMaster;
using System.ComponentModel;

namespace QLDA.Domain.Entities;

/// <summary>
/// Cấu hình vai trò quyền - Role-Permission toggle table
/// Maps roles to permissions with active/inactive toggle
/// </summary>
public class PhanQuyenChucNang : Entity<int>, IAggregateRoot {
    /// <summary>
    /// Tên vai trò (from RoleConstants, e.g., "QLDA_LD", "QLDA_ChuyenVien")
    /// </summary>

    /// <summary>
    /// FK → DanhMucQuyen.Id
    /// </summary>
//   public int? QuyenId { get; set; }// duan.TaoMoi 

    /// <summary>
    /// Bật/tắt quyền cho vai trò
    public bool SuDung { get; set; }
    /// </summary>
    public string MaChucNang { get; set; } = string.Empty;
    public string ChucNang { get; set; }     // ko dùng
    public PhanQuyenChucNangLevel? Level { get; set; }   // NguoiDungMacDinhID, NguoiDungChiDinh, TheoChucVu
    public long? LevelId { get; set; }   //PhongBanId, NguoiDungId, ChucVuId, 
    public bool? NguoiDungMacDinh { get; set; }   
    public List<long>? NguoiDungId { get; set; }   
    #region Navigation Properties
 //   public DanhMucQuyen? Quyen { get; set; }

    #endregion
}

