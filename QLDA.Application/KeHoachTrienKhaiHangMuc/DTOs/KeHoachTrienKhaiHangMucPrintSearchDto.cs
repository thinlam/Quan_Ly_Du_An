using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

/// <summary>
/// Tham số export Excel kế hoạch triển khai hạng mục (PMIS #9469).
/// Cùng bộ filter với danh sách; không truyền id/duAnId → export toàn bộ hạng mục khớp filter.
/// </summary>
public record KeHoachTrienKhaiHangMucPrintSearchDto : IFromDateToDate, IMayHaveGlobalFilter
{
    /// <summary>Id kế hoạch — ưu tiên khi có giá trị.</summary>
    public Guid? Id { get; set; }

    /// <summary>Id dự án — lấy kế hoạch mới nhất của dự án (khi không có id).</summary>
    public Guid? DuAnId { get; set; }

    public int? BuocId { get; set; }
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? GlobalFilter { get; set; }
    public int? TrangThaiId { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public List<string>? HiddenColumns { get; set; }
}
