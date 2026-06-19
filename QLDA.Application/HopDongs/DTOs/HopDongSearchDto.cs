using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.HopDongs.DTOs;

public record HopDongSearchDto : CommonSearchDto {
    public string? Ten { get; set; }
    public string? SoHopDong { get; set; }
    public string? NoiDung { get; set; }
    public int? LoaiHopDongId { get; set; }
    public Guid? DonViThucHienId { get; set; }
    public Guid? TamUngId { get; set; }
    public bool? IsBienBan { get; set; }
    public Guid? GoiThauId { get; set; }
    public Guid? KeHoachLuaChonNhaThauId { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}