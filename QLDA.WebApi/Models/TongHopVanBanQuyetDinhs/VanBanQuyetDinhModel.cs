namespace QLDA.WebApi.Models.TongHopVanBanQuyetDinhs;

public record VanBanQuyetDinhModel  {
    public EnumLoaiVanBanQuyetDinh? Loai { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }

}