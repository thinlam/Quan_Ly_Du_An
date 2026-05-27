using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ToTrinhPheDuyets.DTOs;

public class ToTrinhPheDuyetUpdateDto : IMayHaveTepDinhKemDto {
    public Guid Id { get; set; }
    public string? So { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public DateOnly? NgayToTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}