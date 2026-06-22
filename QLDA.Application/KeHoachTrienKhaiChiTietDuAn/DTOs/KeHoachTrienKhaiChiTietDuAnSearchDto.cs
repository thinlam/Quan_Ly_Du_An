using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;

public record KeHoachTrienKhaiChiTietDuAnSearchDto
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? Ten { get; set; }
    public string? MaMoc { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? GlobalFilter { get; set; }
    public int? TrangThaiId { get; set; }
    public int? DonViChuTriId { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }


}