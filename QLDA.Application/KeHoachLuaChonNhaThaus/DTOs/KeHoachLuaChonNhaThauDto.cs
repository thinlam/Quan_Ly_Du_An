using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.KeHoachLuaChonNhaThaus.DTOs;

public class KeHoachLuaChonNhaThauDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo, IQuyetDinh, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? Ten { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? NguoiKy { get; set; }
    public long TongDuToan { get; set; }
    public long? DuToanThamDinh { get; set; }
    public int? NguonVonId { get; set; }
    public int? ThoiGianThucHien { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
