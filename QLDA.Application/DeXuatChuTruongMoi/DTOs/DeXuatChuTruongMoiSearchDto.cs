using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.DeXuatChuTruongMois.DTOs;

public record DeXuatChuTruongMoiSearchDto : CommonSearchDto
{
    public long? TongMucDauTu { get; set; }
    public int? HinhThucDauTuId { get; set; }
    public long? DonViPhuTrachId { get; set; }
    public long? LanhDaoPhuTrachId { get; set; }
    public string? Loai { get; set; }
    public int? Nam { get; set; }
    
}