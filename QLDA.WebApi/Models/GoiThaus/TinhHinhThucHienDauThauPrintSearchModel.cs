namespace QLDA.WebApi.Models.GoiThaus;

/// <summary>
/// Search model cho print/export báo cáo tình hình thực hiện đấu thầu — không phân trang
/// </summary>
public record TinhHinhThucHienDauThauPrintSearchModel
{
    /// <summary>
    /// Tab loại (<see cref="QLDA.Domain.Enums.TinhHinhThucHienDauThauLoai"/>).
    /// Null hoặc 0 (TatCa) = xuất 3 sheet (mỗi tab một sheet).
    /// </summary>
    public int? Loai { get; set; }
    public List<string>? HiddenColumns { get; set; }
}
