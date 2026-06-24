using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.PhanQuyenChucNangs.DTOs;

public class PhanQuyenChucNangInsUpdDto : IMayHaveTepDinhKemDto, ITienDo {
    public Guid? Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? LoaiDeXuat { get; set; }
    public string? SoToTrinh { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public string? ButPhe { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}