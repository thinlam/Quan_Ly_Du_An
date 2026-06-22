namespace QLDA.Application.PhanKhaiKinhPhis.DTOs;

public class PhanKhaiKinhPhiImportResultDto {
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = [];
}
