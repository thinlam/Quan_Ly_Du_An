using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.BaoCaoKetQuaKhaoSats;

public class BaoCaoKetQuaKhaoSatModel : IHasKey<Guid?>, IMustHaveId<Guid>
{
    public Guid? Id { get; set; }

    public Guid GetId()
    {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungNghiemThu { get; set; }
    public DateOnly NgayKhaoSat { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}
