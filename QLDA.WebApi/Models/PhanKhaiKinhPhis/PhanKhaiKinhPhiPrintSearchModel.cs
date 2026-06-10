namespace QLDA.WebApi.Models.PhanKhaiKinhPhis;

/// <summary>
/// Search model cho print/export phân khai kinh phí — không phân trang
/// </summary>
public record PhanKhaiKinhPhiPrintSearchModel {
    public Guid? DuAnId { get; set; }
    public string? GlobalFilter { get; set; }
    public List<string>? HiddenColumns { get; set; }
}
