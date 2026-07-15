using System.ComponentModel.DataAnnotations;

namespace QLDA.Application.PhanKhaiKinhPhis.DTOs;

public class PhanKhaiKinhPhiImportDto {
    [Required]
    [Description("Dự án")]
    public string? TenDuAn { get; set; }

    [Description("Nguồn vốn")]
    public string? TenNguonVon { get; set; }

    [Description("Kinh phí đề xuất")]
    public double? KinhPhiDeXuat { get; set; }

    [Description("Kinh phí phân khai")]
    public double? KinhPhiPhanKhai { get; set; }

    [Description("Thuyết minh phân khai")]
    public string? ThuyetMinhPhanKhai { get; set; }

    [Description("Số tờ trình")]
    public string? SoToTrinh { get; set; }

    [Description("Ngày tờ trình")]
    public DateTimeOffset? NgayToTrinh { get; set; }

    [Description("Trích yếu")]
    public string? TrichYeu { get; set; }

    public int ExcelRowNumber { get; set; }
}
