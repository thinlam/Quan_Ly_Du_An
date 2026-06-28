namespace QLDA.Application.DuAns.DTOs;

public class TheoDoiDuAnPhongPhanCongDto
{
    public int Stt { get; set; }
    public Guid Id { get; set; }
    public string? MaDuAn { get; set; }
    public string? TenDuAn { get; set; }
    /// <summary>VD: "2026 - 2027"</summary>
    public string? ThoiGianThucHien { get; set; }
    public string? HinhThucQuanLyDuAn { get; set; }
    public string? HinhThucDauTu { get; set; }
    public long? TongMucDauTu { get; set; }
    public string? LanhDaoPhuTrach { get; set; }
    public string? DonViPhuTrachChinh { get; set; }

    public int? HinhThucQuanLyDuAnId { get; set; }
    public int? HinhThucDauTuId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public int? ThoiGianHoanThanh { get; set; }
    public int? TrangThaiDuAnId { get; set; }
}
