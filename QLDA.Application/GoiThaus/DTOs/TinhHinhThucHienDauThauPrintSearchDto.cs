namespace QLDA.Application.GoiThaus.DTOs;

/// <summary>
/// Search DTO cho print/export báo cáo tình hình thực hiện đấu thầu — không phân trang
/// </summary>
public record TinhHinhThucHienDauThauPrintSearchDto
{
    /// <summary>
    /// Tab loại (<see cref="QLDA.Domain.Enums.TinhHinhThucHienDauThauLoai"/>).
    /// Null hoặc 0 (TatCa) = xuất 3 sheet.
    /// </summary>
    public int? Loai { get; set; }

    /// <summary>
    /// Năm dự án — bind query param <c>namDuAn</c>. Null hoặc &lt;= 0 = không lọc.
    /// Cùng logic với param <c>nam</c> của API danh sách
    /// (<see cref="QLDA.Application.GoiThaus.GoiThauTinhHinhDauThauQueryableExtensions.ApplyTinhHinhDauThauNamFilter"/>,
    /// theo <c>DuAn.NgayBatDau</c>) để Excel khớp dữ liệu grid.
    /// </summary>
    public int? NamDuAn { get; set; }

    /// <summary>
    /// Giai đoạn hiện tại (<c>DuAn.GiaiDoanHienTaiId</c>). Null hoặc -1 = không lọc.
    /// </summary>
    public int? GiaiDoanId { get; set; }

    /// <summary>
    /// Dự án — bind query param <c>duAnId</c>. Null = không lọc.
    /// </summary>
    public Guid? DuAnId { get; set; }

    public List<string>? HiddenColumns { get; set; }
}
