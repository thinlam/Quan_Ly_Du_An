namespace QLDA.Application.GoiThaus.DTOs;

public class GoiThauImportResultDto {
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = [];
}
