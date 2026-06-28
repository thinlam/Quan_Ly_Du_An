using QLDA.Domain.Entities.DanhMuc;
using System.ComponentModel;

namespace QLDA.Domain.Entities;

[DisplayName("Quyết định thành lập Ban quản lý dự án")]
public class QuyetDinhLapBanQLDA : VanBanQuyetDinh {


    public int? TrangThaiId { get; set; }
    public string? SoDuThao {  get; set; }
    public string? TrichYeuDuThao {  get; set; }
    #region Navigation Properties

    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    public ICollection<ThanhVienBanQLDA> ThanhViens { get; set; } = [];

    #endregion
}
