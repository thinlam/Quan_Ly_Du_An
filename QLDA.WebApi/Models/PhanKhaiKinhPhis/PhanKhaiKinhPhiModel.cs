using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.Common.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.PhanKhaiKinhPhis;

public class PhanKhaiKinhPhiModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel {
    public Guid? Id { get; set; }
    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid SetId() => SequentialGuidGenerator.Instance.NewGuid();

    public Guid DuAnId { get; set; }
    public string? SoToTrinh { get; set; }
    public DateTimeOffset? NgayToTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public int? NguonVonId { get; set; }
    public decimal? KinhPhiDeXuat { get; set; }
    public decimal? KinhPhiPhanKhai { get; set; }
    public string? ThuyetMinh { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}