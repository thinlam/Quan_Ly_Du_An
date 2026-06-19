using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.ToTrinhPheDuyets.DTOs;

public record ToTrinhPheDuyetSearchDto : CommonSearchDto
{
    public string? SoQuyetDinh { get; set; }
    public string? Loai { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}