using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QLDA.WebApi.Models.GoiThaus;

public class GoiThauImportModel {
    [Description("STT")]
    public int? Stt { get; set; }

    [Required]
    [Description("Tên dự án")]
    public string TenDuAn { get; set; } = string.Empty;

    [Description("Kế hoạch lựa chọn nhà thầu")]
    public string? TenKeHoachLuaChonNhaThau { get; set; }

    [Description("Tên bước")]
    public string? TenBuoc { get; set; }

    [Description("Tên gói thầu")]
    public string? Ten { get; set; }

    [Description("Tóm tắt công việc chính")]
    public string? TomTatCongViecChinhGoiThau { get; set; }

    [Description("Giá gói thầu")]
    public long? GiaTri { get; set; }

    [Description("Nguồn vốn")]
    public string? TenNguonVon { get; set; }

    [Description("Hình thức lựa chọn nhà thầu")]
    public string? TenHinhThucLuaChonNhaThau { get; set; }

    [Description("Phương thức lựa chọn nhà thầu")]
    public string? TenPhuongThucLuaChonNhaThau { get; set; }

    [Description("Loại hợp đồng")]
    public string? TenLoaiHopDong { get; set; }

    [Description("Thời gian tổ chức lựa chọn nhà thầu")]
    public string? ThoiGianToChucLuaChonNhaThau { get; set; }

    [Description("Thời gian bắt đầu tổ chức lựa chọn nhà thầu")]
    public string? ThoiGianBatDauToChucLuaChonNhaThau { get; set; }

    [Description("Thời gian thực hiện nhà thầu (ngày)")]
    public int? ThoiGianThucHienGoiThau { get; set; }

    [Description("Tùy chọn mua thêm")]
    public string? TuyChonMuaThem { get; set; }

    [Description("Giám sát hoạt động đấu thầu")]
    public string? GiamSatHoatDongDauThau { get; set; }
}