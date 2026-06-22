namespace QLDA.WebApi.Models.QuyetDinhLapBanQLDAs;

public record QuyetDinhLapBanQldaSearchModel : CommonSearchModel, IFromDateToDate {
    public string? SoQuyetDinh { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
}