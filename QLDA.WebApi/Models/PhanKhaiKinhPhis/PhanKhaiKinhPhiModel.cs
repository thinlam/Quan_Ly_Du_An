using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.WebApi.Models.PhanKhaiKinhPhis;

public class PhanKhaiKinhPhiModel : IHasKey<Guid?>, IMustHaveId<Guid> {
    public Guid? Id { get; set; }
    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid SetId() => SequentialGuidGenerator.Instance.NewGuid();

    public Guid DuAnId { get; set; }
    public string? SoToTrinh { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public int? NguonVonId { get; set; }
    public long? KinhPhiDeXuat { get; set; }
    public long? KinhPhiPhanKhai { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }
}
