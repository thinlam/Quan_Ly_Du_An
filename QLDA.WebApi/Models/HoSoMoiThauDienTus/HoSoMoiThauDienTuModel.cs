using SequentialGuid;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoMoiThauDienTus;

public class HoSoMoiThauDienTuModel : IHasKey<Guid?>, IMustHaveId<Guid>,
    IMayHaveTepDinhKemModel {

    public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }
    public Guid? GoiThauId { get; set; }
    public long? GiaTri { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public bool TrangThaiDangTai { get; set; }
    public int? TrangThaiId { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}