namespace QLDA.WebApi.Models.DeXuatNhuCauKinhPhis;

/// <summary>
/// Search model cho print/export danh mục xin chủ trương đầu tư — không phân trang
/// </summary>
public record DeXuatNhuCauKinhPhiPrintSearchModel {
    public Guid? DuAnId { get; set; }
    public int? TrangThaiId { get; set; }
    public string? GlobalFilter { get; set; }
    public List<string>? HiddenColumns { get; set; }
}
