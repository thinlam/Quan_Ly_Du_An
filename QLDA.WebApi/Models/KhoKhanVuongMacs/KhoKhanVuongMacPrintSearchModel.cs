namespace QLDA.WebApi.Models.KhoKhanVuongMacs;

/// <summary>
/// Search model cho print/export khó khăn vướng mắc — không phân trang
/// </summary>
public record KhoKhanVuongMacPrintSearchModel {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public List<string>? HiddenColumns { get; set; }
    public string? NoiDung { get; set; }
    public int? TinhTrangId { get; set; }
    public int? MucDoKhoKhanId { get; set; }
    public int? LoaiDuAnId { get; set; }
    public long? LanhDaoPhuTrachId { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
}
