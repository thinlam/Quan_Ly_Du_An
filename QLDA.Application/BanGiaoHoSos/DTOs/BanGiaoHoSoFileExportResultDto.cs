namespace QLDA.Application.BanGiaoHoSos.DTOs;

public class BanGiaoHoSoFileExportResultDto
{
    public string? TenDuAn { get; set; }
    public List<BanGiaoHoSoFileExportItemDto> Files { get; set; } = [];
}

public class BanGiaoHoSoFileExportItemDto
{
    public string? TenFile { get; set; }
    public DateTimeOffset ThoiGianDinhKem { get; set; }
}
