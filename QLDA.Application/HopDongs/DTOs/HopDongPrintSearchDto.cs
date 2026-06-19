namespace QLDA.Application.HopDongs.DTOs;

/// <summary>
/// Search DTO cho print/export hợp đồng — không phân trang
/// </summary>
public record HopDongPrintSearchDto {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public List<string>? HiddenColumns { get; set; }
    public string? Ten { get; set; }
    public string? SoHopDong { get; set; }
    public string? NoiDung { get; set; }
    public int? LoaiHopDongId { get; set; }
    public Guid? DonViThucHienId { get; set; }
    public bool? IsBienBan { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}
