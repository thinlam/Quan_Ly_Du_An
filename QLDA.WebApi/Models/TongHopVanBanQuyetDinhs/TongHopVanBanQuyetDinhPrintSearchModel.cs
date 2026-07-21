namespace QLDA.WebApi.Models.TongHopVanBanQuyetDinhs;

/// <summary>
/// Search model cho print/export tổng hợp văn bản quyết định
/// </summary>
public record TongHopVanBanQuyetDinhPrintSearchModel : CommonSearchModel, IFromDateToDate
{
    public EnumLoaiVanBanQuyetDinh? Loai { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
}
