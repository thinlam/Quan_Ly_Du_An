using QLDA.Application.Common.Interfaces;
using QLDA.Application.PhuLucHopDongs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.ThanhToans.DTOs;

public class ThanhToanDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid NghiemThuId { get; set; }
    public Guid HopDongId { get; set; }
    public List<Guid>? PhuLucHopDongIds { get; set; }
    public string? SoHoaDon { get; set; }
    public DateTimeOffset? NgayHoaDon { get; set; }
    public long? GiaTri { get; set; }
    public string? NoiDung { get; set; }
    public List<phuLucHopDongCboDto>? PhuLucs { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}