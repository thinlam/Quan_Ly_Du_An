namespace QLDA.WebApi.Models.TongHopVanBanQuyetDinhs;

public record TongHopVanBanQuyetDinhSearchModel : CommonSearchModel, IFromDateToDate {
    public EnumLoaiVanBanQuyetDinh? Loai { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
}