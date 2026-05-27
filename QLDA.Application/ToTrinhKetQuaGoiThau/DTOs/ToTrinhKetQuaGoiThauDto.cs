using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using SequentialGuid;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.DTOs;

public class ToTrinhKetQuaGoiThauDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? NgayTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public string? TenTrangThaiDangTai { get; set; }
    public List<Guid>? DanhSachGoiThau { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
