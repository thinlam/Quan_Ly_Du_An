namespace QLDA.WebApi.Models.DeXuatNhuCauKinhPhis;

/// <summary>
/// Search model cho print/export tổng hợp nhu cầu kinh phí năm — không phân trang
/// </summary>
public record TheoDoiDeXuatNhuCauKinhPhiPrintSearchModel {
    public Guid? DuAnId { get; set; }
    public int? TrangThaiId { get; set; }
    public int? TrangThaiKeHoachNamId { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public string? TrichYeu { get; set; }
    public string? GlobalFilter { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public long? DonViDeXuatId { get; set; }
    public List<string>? HiddenColumns { get; set; }
}
