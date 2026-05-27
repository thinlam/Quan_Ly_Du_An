using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.ToTrinhPheDuyets.DTOs;

public class ToTrinhPheDuyetInsertDto : IMayHaveTepDinhKemDto, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public string Loai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }

}