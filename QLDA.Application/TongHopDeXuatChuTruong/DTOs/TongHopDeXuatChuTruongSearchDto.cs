using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.TongHopDeXuatChuTruongs.DTOs;

public record TongHopDeXuatChuTruongSearchDto : CommonSearchDto
{
    public string? LoaiDeXuat { get; set; } 
   // public long? TongMucDauTu { get; set; }
    public long? PhongBanDeXuatId { get; set; }
    //public long? DonViPhuTrachId { get; set; }
   // public long? LanhDaoPhuTrachId { get; set; }
}