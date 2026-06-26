namespace QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

public class KeHoachTrienKhaiHangMucImportResultDto {
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = [];
    public byte[]? ErrorFileBytes { get; set; }
    public string? ErrorFileName { get; set; }
}
