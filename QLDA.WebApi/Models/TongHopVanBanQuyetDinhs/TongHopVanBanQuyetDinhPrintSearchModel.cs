using QLDA.Domain.Enums;

namespace QLDA.WebApi.Models.TongHopVanBanQuyetDinhs;

/// <summary>
/// Search model cho print/export tổng hợp văn bản quyết định — không phân trang
/// </summary>
public record TongHopVanBanQuyetDinhPrintSearchModel {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public List<string>? HiddenColumns { get; set; }
    public EnumLoaiVanBanQuyetDinh? Loai { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
}
