namespace QLDA.WebApi.Models.PhanKhaiKinhPhis;

public record PhanKhaiKinhPhiSearchModel : CommonSearchModel {
    public string? TenDuAn { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public int? TrangThaiId { get; set; }
}
