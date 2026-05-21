using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;

public class DeXuatNhuCauKinhPhiDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public long? DonViDeXuatId { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public DateTimeOffset? NgayPhieuChuyen { get; set; }
    public string? TrichYeu { get; set; }
    public int? TrangThaiId { get; set; }
    public long? KinhPhiDeXuat { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}