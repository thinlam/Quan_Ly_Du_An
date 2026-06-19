namespace QLDA.WebApi.Models.DeXuatChuTruongChuyenTieps;

/// <summary>
/// Search model cho print/export đề xuất chủ trương chuyển tiếp — không phân trang
/// </summary>
public record DeXuatChuyenTiepPrintSearchModel {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public List<string>? HiddenColumns { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
}
