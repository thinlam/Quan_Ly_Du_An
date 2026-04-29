using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Cấu hình vai trò quyền - Role-Permission toggle table
/// Maps roles to permissions with active/inactive toggle
/// </summary>
public class CauHinhVaiTroQuyen : Entity<int>, IAggregateRoot {
    /// <summary>
    /// Tên vai trò (from RoleConstants, e.g., "QLDA_LD", "QLDA_ChuyenVien")
    /// </summary>
    public string VaiTro { get; set; } = string.Empty;

    /// <summary>
    /// FK → DanhMucQuyen.Id
    /// </summary>
    public int QuyenId { get; set; }

    /// <summary>
    /// Bật/tắt quyền cho vai trò
    /// </summary>
    public bool KichHoat { get; set; } = true;

    #region Navigation Properties

    public DanhMucQuyen? Quyen { get; set; }

    #endregion
}
