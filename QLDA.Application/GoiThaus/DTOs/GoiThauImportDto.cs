namespace QLDA.Application.GoiThaus.DTOs;

public class GoiThauImportDto
{
    /// <summary>
    /// Kế hoạch lựa chọn nhà thầu
    /// </summary>
    public string? TenKeHoachLuaChonNhaThau { get; set; }
    /// <summary>
    /// Gói thầu
    /// </summary>
    public string? Ten { get; set; }
    /// <summary>
    /// Tóm tắt công việc chính của gói thầu
    /// </summary>
    public string? TomTatCongViecChinhGoiThau { get; set; }
    /// <summary>
    /// Giá trị gói thầu
    /// </summary>
    public long? GiaTri { get; set; }
    /// <summary>
    /// Nguồn vốn
    /// </summary>
    public string? TenNguonVon { get; set; }
    /// <summary>
    /// Hình thức lựa chọn nhà thầu
    /// </summary>
    public string? TenHinhThucLuaChonNhaThau { get; set; }
    /// <summary>
    /// Phương thức lựa chọn nhà thầu
    /// </summary>
    public string? TenPhuongThucLuaChonNhaThau { get; set; }
    /// <summary>
    /// Thời gian tổ chức lựa chọn nhà thầu
    /// </summary>Th
    public string? ThoiGianToChucLuaChonNhaThau { get; set; }
    /// <summary>
    /// Thời gian bắt đầu tổ chức lựa chọn nhà thầu
    /// </summary>
    public string? ThoiGianBatDauToChucLuaChonNhaThau { get; set; }
    /// <summary>
    /// Loại hợp đồng
    /// </summary>
    public string? TenLoaiHopDong { get; set; }
    /// <summary>
    /// Thời gian thực hiện gói thầu (ngày)
    /// </summary>
    public int? ThoiGianThucHienGoiThau { get; set; }
    public string? TuyChonMuaThem { get; set; }
}