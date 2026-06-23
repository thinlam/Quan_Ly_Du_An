using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.ToTrinhPheDuyets.DTOs;

public class ToTrinhPheDuyetExportDto : ToTrinhPheDuyetDto
{
    
    public long? DonViTrinhId { get; set; }    
    public string? TenDuAn { get; set; }    
    public string? TenLanhDaoPhuTrach { get; set; }    
    public string?  TenDonViTrinh { get; set; } 
    public string? TenNguoiTrinh { get; set; }
}