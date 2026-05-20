namespace QLDA.Application.DanhMucLoaiDieuChinhs.DTOs;

public record DanhMucLoaiDieuChinhDto {
    public int Id { get; init; }
    public string Ma { get; init; } = string.Empty;
    public string Ten { get; init; } = string.Empty;
    public string? MoTa { get; init; }
    public int? Stt { get; init; }
    public bool Used { get; init; }
}

public record DanhMucLoaiDieuChinhInsertDto {
    public string Ma { get; init; } = string.Empty;
    public string Ten { get; init; } = string.Empty;
    public string? MoTa { get; init; }
    public int? Stt { get; init; }
    public bool Used { get; init; }
}

public record DanhMucLoaiDieuChinhUpdateDto {
    public int Id { get; init; }
    public string Ma { get; init; } = string.Empty;
    public string Ten { get; init; } = string.Empty;
    public string? MoTa { get; init; }
    public int? Stt { get; init; }
    public bool Used { get; init; }
}