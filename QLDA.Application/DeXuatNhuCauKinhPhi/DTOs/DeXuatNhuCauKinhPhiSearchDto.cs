using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;

public record DeXuatNhuCauKinhPhiSearchDto : CommonSearchDto
{
    public long? DonViDeXuatId { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public string? TrichYeu { get; set; }
    public int? TrangThaiId { get; set; }
    public int? TrangThaiTongHopId { get; set; }
}