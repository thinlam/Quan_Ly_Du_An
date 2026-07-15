namespace QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

public class KeHoachTrienKhaiHangMucImportResultDto {
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = [];
    public byte[]? ErrorFileBytes { get; set; }
    public string? ErrorFileName { get; set; }

    /// <summary>Id bản ghi kế hoạch triển khai vừa tạo (khi chỉ tạo 1 bản ghi).</summary>
    public Guid? Id { get; set; }

    /// <summary>Danh sách id kế hoạch triển khai vừa tạo (khi import tạo nhiều bản ghi).</summary>
    public List<Guid> Ids { get; set; } = [];

    public string? Message { get; set; }
}
