namespace QLDA.WebApi.Models.CanBoTrienKhaiHangMucs;


public class HangMucTrienKhaiModel
{
    public string TenHangMuc { get; set; } = string.Empty;
    public int? GiaiDoanId { get; set; }
    public long? CanBoChuTriId { get; set; }
    public string? TenCanBoChuTri { get; set; }
    public List<long>? CanBoPhoiHopId { get; set; }
    public long? DonViChuTriId { get; set; }
    public string? TenDonViChuTri { get; set; }
    public List<long>? DonViPhoiHopId { get; set; }
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public DateOnly? ThoiHan { get; set; }
    public long? KinhPhi { get; set; }
}