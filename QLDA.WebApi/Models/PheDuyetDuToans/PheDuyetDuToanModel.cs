using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.PheDuyetDuToans;

public class PheDuyetDuToanModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel, ITienDo {
    public Guid? Id { get; set; }
    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid SetId() {
        
        return SequentialGuidGenerator.Instance.NewGuid();
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? SoVanBan { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? NguoiKy { get; set; }
    public int? ChucVuId { get; set; }
    public long? GiaTriDuThau { get; set; }
    public string? TrichYeu { get; set; }
    public int TrangThaiId { get; set; } = 1;
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}