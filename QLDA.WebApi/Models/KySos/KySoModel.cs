using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.KySos;

public class KySoModel : IMayHaveTepDinhKemModel {
    public Guid GroupId { get; set; }
    public string? GroupName { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}