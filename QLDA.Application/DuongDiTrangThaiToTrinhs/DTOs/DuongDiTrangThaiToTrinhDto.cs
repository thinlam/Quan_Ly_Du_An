using QLDA.Domain.Constants;

namespace QLDA.Application.DuongDiTrangThaiToTrinhs.DTOs;

public class DuongDiTrangThaiToTrinhDto{
    
        public string? MaTrangThaiHienTai { get; set; }
        public string? MaTrangThaiTiepTheo { get; set; }
    public string? TenTrangThaiTiepTheo { get; set; }
    //   RoleLevel : 1 là Phòng ban Chủ trì, 2 là User chỉ định (UserPortalId), 3 Là Phòng Ban chỉ định, 4 là đơn vị chỉ định
    public long? RoleId { get; set; }
    public DuongDiToTrinhRoleLevel? RoleLevel { get; set; }
   
}