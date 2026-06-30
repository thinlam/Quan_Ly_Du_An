using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

public class KeHoachTrienKhaiHangMucImportDto {
    [Required]
    [Description("Dự án")]
    public string? TenDuAn { get; set; }

    [Required]
    [Description("Tên hạng mục")]
    public string? TenHangMuc { get; set; }

    [Required]
    [Description("Giai đoạn")]
    public string? TenGiaiDoan { get; set; }

    [Required]
    [Description("Đơn vị chủ trì")]
    public string? TenDonViChuTri { get; set; }

    [Description("Đơn vị phối hợp")]
    public string? TenDonViPhoiHop { get; set; }

    [Required]
    [Description("Cán bộ chủ trì")]
    public string? TenCanBoChuTri { get; set; }

    [Description("Cán bộ phối hợp")]
    public string? TenCanBoPhoiHop { get; set; }

    [Description("Ngày bắt đầu")]
    public DateOnly? NgayBatDau { get; set; }

    [Description("Ngày kết thúc")]
    public DateOnly? NgayKetThuc { get; set; }

    [Description("Kinh phí")]
    public long? KinhPhi { get; set; }

    [Description("Thời hạn hoàn thành")]
    public DateOnly? ThoiHan { get; set; }

    public int ExcelRowNumber { get; set; }

    public string? ErrorMessage { get; set; }
}
