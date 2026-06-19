namespace QLDA.WebApi.Models.QuyetDinhLapHoiDongThamDinhs;

public record QuyetDinhLapHoiDongThamDinhSearchModel : CommonSearchModel, IFromDateToDate {
    public string? SoQuyetDinh { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
}