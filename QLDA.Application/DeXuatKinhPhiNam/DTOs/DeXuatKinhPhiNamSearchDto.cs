namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;

public record DeXuatNhuCauKinhPhiNamSearchDto 
{
    public long? PhongBanDeXuatId { get; set; }
    public long? NguoiDeXuatId { get; set; }
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? GlobalFilter { get; set; }
    public int? TrangThaiId { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }


}