namespace QLDA.WebApi.Models.DanhMucPhuongThucKySos;

public static class DanhMucPhuongThucKySoMappingConfiguration {
    public static DanhMucPhuongThucKySoModel ToModel(this DanhMucPhuongThucKySo entity) => new() {
        Id = entity.Id,
        Ma = entity.Ma,
        Ten = entity.Ten,
        MoTa = entity.MoTa,
        Stt = entity.Stt,
        Used = entity.Used
    };

    public static DanhMucPhuongThucKySo ToEntity(this DanhMucPhuongThucKySoModel model) => new() {
        Id = model.GetId(),
        Ma = model.Ma,
        Ten = model.Ten,
        MoTa = model.MoTa,
        Stt = model.Stt,
        Used = model.Used
    };
}