using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.DTOs;

public class ToTrinhKetQuaGoiThauInsertDto : IMayHaveTepDinhKemDto
{
    [DefaultValue(null)] public Guid? Id { get; set; }
  
    public string? So { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public DateTimeOffset? NgayTrinh{ get; set; }
    public string? TrichYeu { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<Guid>? DanhSachGoiThau { get; set; }


}