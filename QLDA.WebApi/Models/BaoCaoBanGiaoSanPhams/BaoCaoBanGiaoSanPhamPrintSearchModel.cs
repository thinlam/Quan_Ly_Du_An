namespace QLDA.WebApi.Models.BaoCaoBanGiaoSanPhams;

/// <summary>
/// Search model cho print/export báo cáo bàn giao sản phẩm — không phân trang
/// </summary>
public record BaoCaoBanGiaoSanPhamPrintSearchModel {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public List<string>? HiddenColumns { get; set; }
    public string? NoiDung { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
}
