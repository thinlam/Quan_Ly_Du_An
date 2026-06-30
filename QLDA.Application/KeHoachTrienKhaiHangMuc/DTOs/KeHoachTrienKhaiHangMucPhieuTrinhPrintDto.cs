namespace QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

public class KeHoachTrienKhaiHangMucPhieuTrinhPrintDto
{
    public string So { get; set; } = string.Empty;
    public DateTimeOffset? NgayToTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public string? MaDuAn { get; set; }
    public string? TenDuAn { get; set; }

    /// <summary>Format sẵn: "{MaDuAn} — {TenDuAn}"</summary>
    public string DuAnDisplay { get; set; } = string.Empty;

    public List<KeHoachTrienKhaiHangMucExportItemDto> Rows { get; set; } = [];
}
