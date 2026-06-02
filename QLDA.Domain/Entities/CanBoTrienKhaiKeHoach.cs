using BuildingBlocks.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng quản lý hồ sơ mời thầu điện tử
/// </summary>
public class CanBoTrienKhaiHangMuc {
    public Guid KeHoachId { get; set; }
    public long CanBoId { get; set; }
    public KeHoachTrienKhaiHangMuc KeHoachTrienKhai { get; set; }
    public UserMaster? CanBo { get; set; } // Navigation property by      CanBoId map public long? UserPortalId of CanBo

}