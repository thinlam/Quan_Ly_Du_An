using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.Common.DTOs;

/// <summary>
/// Base search model cho print/export endpoints — không phân trang
/// </summary>
public abstract record PrintSearchDto : IFromDateToDate, IMayNeedHiddenColumns {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public List<string>? HiddenColumns { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
}
