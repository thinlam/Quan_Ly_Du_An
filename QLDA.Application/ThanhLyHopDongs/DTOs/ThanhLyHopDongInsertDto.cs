using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.ThanhLyHopDongs.DTOs;

public class ThanhLyHopDongInsertDto : IMayHaveTepDinhKemInsertDto, ITienDo {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? HopDongId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }
    public List<Guid>? NghiemThuIds { get; set; }
    public List<TepDinhKemInsertDto>? DanhSachTepDinhKem { get; set; }
}
