using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.ThuyetMinhDuAns.DTOs;

public class ThuyetMinhDuAnDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public DateTimeOffset NgayTrinh { get; set; }
    public string So { get; set; } = string.Empty;
    public string? TrichYeu { get; set; }

    public int? TrangThaiThamDinhId { get; set; }
    public string? MaTrangThaiThamDinh { get; set; }
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public string? TenTrangThaiThamDinh { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}