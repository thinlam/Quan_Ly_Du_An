using SequentialGuid;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoDeXuatCapDoCntts;

public class HoSoDeXuatCapDoCnttModel : IHasKey<Guid?>, IMustHaveId<Guid>,
    IMayHaveTepDinhKemModel {

    public Guid? Id { get; set; }
    
    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? TrangThaiId { get; set; }
    public int? CapDoId { get; set; }
    public DateOnly? NgayTrinh { get; set; }
    public int? DonViChuTriId { get; set; }
    public string? NoiDungDeNghi { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungDuThao { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}