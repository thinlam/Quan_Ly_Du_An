using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.ChuTruongLapKeHoachs.DTOs;

public class ChuTruongLapKeHoachDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public string? TenDuAn { get; set; }
    public int? BuocId { get; set; }
    public int? LoaiDeXuat { get; set; }
    public string? TenLoaiDeXuat { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }

    public string? SoToTrinh { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public string? ButPhe { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}