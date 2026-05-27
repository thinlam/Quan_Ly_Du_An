using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.ToTrinhPheDuyets.DTOs;

public class ToTrinhPheDuyetDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public string So { get; set; } = string.Empty;
    public string? TrichYeu { get; set; }
    public string Loai { get; set; }
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}