using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.PhanQuyenChucNangs.DTOs;

public class PhanQuyenChucNangDto : IHasKey<int?> {
    [DefaultValue(null)] public int? Id { get; set; }
    
    public bool SuDung { get; set; }
    public string? TenLevel { get; set; }
    public string? MaChucNang { get; set; }
    public string? ChucNang { get; set; }
    public PhanQuyenChucNangLevel? Level { get; set; }
    public long? LevelId { get; set; } // chức vụ, tên phòng ban
    public List<PhanQuyenChucNangCapDoDto> DanhSachChiTiet { get; set; }
    // public bool? NguoiDungMacDinh { get; set; } // đối tượng
    //public List<long>  DanhSachNguoiDung { get; set; }
}