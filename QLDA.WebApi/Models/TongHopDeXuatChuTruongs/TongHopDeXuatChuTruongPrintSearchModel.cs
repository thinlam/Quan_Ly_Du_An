namespace QLDA.WebApi.Models.TongHopDeXuatChuTruongs;

public record TongHopDeXuatChuTruongPrintSearchModel {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public string? Loai { get; set; }
    public int? Nam { get; set; }
    public long? DonViPhuTrachId { get; set; }
    public List<string>? HiddenColumns { get; set; }
}
