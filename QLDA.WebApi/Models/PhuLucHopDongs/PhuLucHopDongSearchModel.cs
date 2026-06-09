namespace QLDA.WebApi.Models.PhuLucHopDongs;

public record PhuLucHopDongSearchModel : CommonSearchModel, IFromDateToDate {
    public string? Ten { get; set; }
    public string? SoPhuLucHopDong { get; set; }
    public string? NoiDung { get; set; }
    public Guid? HopDongId { get; set; }
    public Guid? ThanhToanId { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
}