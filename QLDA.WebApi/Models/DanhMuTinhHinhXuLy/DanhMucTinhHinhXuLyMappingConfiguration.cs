namespace QLDA.WebApi.Models.DanhMucTinhHinhXuLys;

public static class DanhMucTinhHinhXuLyMappingConfiguration
{
    public static DanhMucTinhHinhXuLyModel ToModel(this DanhMucTinhHinhXuLy entity)
        => new() {
            Id = entity.Id,
            Ma = entity.Ma,
            Ten = entity.Ten,
            MoTa = entity.MoTa,
            Stt = entity.Stt,Used = entity.Used
        };

    public static DanhMucTinhHinhXuLy ToEntity(this DanhMucTinhHinhXuLyModel model)
        => new() {
            Id = model.GetId(),
            Ma = model.Ma,
            Ten = model.Ten,
            MoTa = model.MoTa,
            Stt = model.Stt,Used = model.Used
        };
}