//using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.PhanQuyenChucNangs.DTOs;

public record PhanQuyenChucNangSearchDto : CommonSearchDto
{
    
  //  public int QuyenId { get; set; } // DuAn.TaoMoi/ DuAn.Sua/GoiThau
    public bool SuDung { get; set; }
    public string? MaChucNang { get; set; }
    public string? ChucNang { get; set; }
    public int? Level { get; set; }   // PhanQuyenChucNangLevel NguoiDungMacDinhID, NguoiDungChiDinh, TheoChucVu
  //  public long? LevelId { get; set; }   //PhongBanId, NguoiDungId, ChucVuId, 
}