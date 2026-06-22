namespace QLDA.WebApi.Models.KhoKhanVuongMacs;

public record KhoKhanVuongMacSearchModel : CommonSearchModel, IFromDateToDate {
    public string? NoiDung { get; set; }
    public int? TinhTrangId { get; set; }
    public int? MucDoKhoKhanId { get; set; }
    public int? LoaiDuAnId { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public long? LanhDaoPhuTrachId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public long? DonViPhoiHopId { get; set; }
}