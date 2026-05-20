using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.GoiThaus.DTOs;

/// <summary>
/// Search DTO cho print/export gói thầu — không phân trang
/// </summary>
public record GoiThauPrintSearchDto {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public List<string>? HiddenColumns { get; set; }
    public string? Ten { get; set; }
    public Guid? HopDongId { get; set; }
    public int? NguonVonId { get; set; }
    public int? LoaiHopDongId { get; set; }
    public int? LoaiGoiThauId { get; set; }
    public int? PhuongThucLuaChonNhaThauId { get; set; }
    public Guid? KeHoachLuaChonNhaThauId { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }
}
