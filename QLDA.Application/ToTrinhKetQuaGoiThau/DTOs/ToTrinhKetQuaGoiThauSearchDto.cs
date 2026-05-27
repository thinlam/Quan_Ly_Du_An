using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.DTOs;

public record ToTrinhKetQuaGoiThauSearchDto
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? GlobalFilter { get; set; }
    public int? TrangThaiId { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }


}