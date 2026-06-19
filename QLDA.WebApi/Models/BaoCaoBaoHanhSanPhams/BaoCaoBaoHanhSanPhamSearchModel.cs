namespace QLDA.WebApi.Models.BaoCaoBaoHanhSanPhams;

public record BaoCaoBaoHanhSanPhamSearchModel : CommonSearchModel, IFromDateToDate {
    public string? NoiDung { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
}