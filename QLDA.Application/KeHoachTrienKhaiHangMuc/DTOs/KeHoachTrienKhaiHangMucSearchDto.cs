using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

public record KeHoachTrienKhaiHangMucSearchDto
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? GlobalFilter { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenHangMuc { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public bool? IsDuAnChuaCoKeHoach { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }


}