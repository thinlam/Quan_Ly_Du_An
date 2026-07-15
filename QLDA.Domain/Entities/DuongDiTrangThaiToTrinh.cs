using QLDA.Domain.Constants;

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
    public long? RecipientRoleId { get; set; }
    public DuongDiToTrinhRoleLevel? RecipientRoleLevel { get; set; }
    public bool Used { get; set; }
    public bool? IsDeleted { get; set; }

}
