namespace QLDA.Application.DuAns.DTOs;

/// <summary>
/// Search DTO cho print/export dự án — không phân trang
/// </summary>
public record DuAnPrintSearchDto {
    public string? TenDuAn { get; set; }
    public string? MaDuAn { get; set; }
    public int? LinhVucId { get; set; }
    public int? NguonVonId { get; set; }
    public int? NhomDuAnId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public long? DonViPhoiHopId { get; set; }
    public int? GiaiDoanId { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public int? ThoiGianHoanThanh { get; set; }
    public long? LanhDaoPhuTrachId { get; set; }
    public string? MaNganSach { get; set; }
    public int? LoaiDuAnId { get; set; }
    public int? QuyTrinhId { get; set; }
    public int? TrangThaiDuAnId { get; set; }
    public int? NamBatDau { get; set; }
    public int? NamDuAn { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public int? HinhThucDauTuId { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public List<string>? HiddenColumns { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
}
