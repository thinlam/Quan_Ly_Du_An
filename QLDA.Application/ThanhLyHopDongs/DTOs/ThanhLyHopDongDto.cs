using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.ThanhLyHopDongs.DTOs;

public class ThanhLyHopDongDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? HopDongId { get; set; }
    public string? HopDongTen { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TrangThaiTen { get; set; }
    public List<Guid>? NghiemThuIds { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
