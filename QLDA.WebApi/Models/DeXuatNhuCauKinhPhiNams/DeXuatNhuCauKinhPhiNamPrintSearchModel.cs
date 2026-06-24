namespace QLDA.WebApi.Models.DeXuatNhuCauKinhPhiNams;

/// <summary>
/// Search model cho print/export tổng hợp nhu cầu kinh phí năm — không phân trang
/// </summary>
public record DeXuatNhuCauKinhPhiNamPrintSearchModel {
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? TrangThaiId { get; set; }
    public string? GlobalFilter { get; set; }
    public List<string>? HiddenColumns { get; set; }
}
