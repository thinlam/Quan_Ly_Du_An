namespace QLDA.WebApi.Models.BaoCaoBanGiaoSanPhams;

public record BaoCaoBanGiaoSanPhamSearchModel : CommonSearchModel, IFromDateToDate {
    public string? NoiDung { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
}