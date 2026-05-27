using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.DTOs;

public class ToTrinhKetQuaGoiThauUpdateDto : IMayHaveTepDinhKemDto {
    public Guid Id { get; set; }
   
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string So { get; set; }
    public DateTimeOffset? NgayTrinh{ get; set; }
    public string? TrichYeu { get; set; } = string.Empty;
    public int? TrangThaiDangTaiId { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}