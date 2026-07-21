namespace QLDA.Application.PhanQuyenChucNangs.DTOs;

public class PhanQuyenChucNangCapDoDto  {
   
    public long LevelId { get; set; }//PhongBanId,ChucVuid,User_porttalId
    public bool? NguoiDungMacDinh { get; set; } 
    public string? TenNguoiDungMacDinh { get; set; } 
    public List<long>? NguoiDungChiDinhs { get; set; }
}