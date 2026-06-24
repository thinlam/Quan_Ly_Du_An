namespace QLDA.WebApi.Models.TongHopDeXuatChuTruongs;

/// <summary>
/// Search model cho print/export đề xuất nhu cầu kinh phí chủ trương — không phân trang.
/// </summary>
public record TongHopDeXuatNhuCauKinhPhiPrintSearchModel {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public string? Loai { get; set; }
    public int? Nam { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public long? DonViPhuTrachId { get; set; }
    public List<string>? HiddenColumns { get; set; }
}
