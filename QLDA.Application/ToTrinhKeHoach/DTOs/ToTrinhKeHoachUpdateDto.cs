using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ToTrinhKeHoachs.DTOs;

public class ToTrinhKeHoachUpdateDto : IMayHaveTepDinhKemDto {
    public Guid Id { get; set; }
    public string? So { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public DateTime? NgayToTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}