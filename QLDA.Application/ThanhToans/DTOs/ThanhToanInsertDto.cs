using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.ThanhToans.DTOs;

public class ThanhToanInsertDto : IMayHaveTepDinhKemInsertDto, ITienDo {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid NghiemThuId { get; set; }
    public string? SoHoaDon { get; set; }
    public DateTimeOffset? NgayHoaDon { get; set; }
    [DefaultValue(0)] public long? GiaTri { get; set; }
    public string? NoiDung { get; set; }
    public List<Guid>? PhuLucs { get; set; }

    public List<TepDinhKemInsertDto>? DanhSachTepDinhKem { get; set; }
}