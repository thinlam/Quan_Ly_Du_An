using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using SequentialGuid;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;

public class DeXuatNhuCauKinhPhiNamDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public string? So { get; set; }
    public DateTimeOffset? NgayKeHoach { get; set; }
    public string? TrichYeu { get; set; }
    public long? TongKinhPhiDeXuat { get; set; }
    public string? GhiChu { get; set; }
    public string? MaTrangThai { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public List<Guid>? DanhSachDeXuat { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
