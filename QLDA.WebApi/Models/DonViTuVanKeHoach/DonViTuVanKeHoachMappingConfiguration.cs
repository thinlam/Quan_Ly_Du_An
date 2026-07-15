namespace QLDA.WebApi.Models.DonViTuVanKeHoachs;

public static class DonViTuVanKeHoachMappingConfiguration {
    public static DonViTuVanKeHoachModel ToModel(this DonViTuVanKeHoach entity) =>
        new() {
            Id = entity.Id,
            KeHoachId = entity.KeHoachId,
            TenDonVi = entity.TenDonVi ?? string.Empty
        };


    public static DonViTuVanKeHoach ToEntity(this DonViTuVanKeHoachModel model, Guid keHoachId)
        => new() {
            Id = model.Id ?? model.GetId(),
            KeHoachId = keHoachId,
            TenDonVi = model.TenDonVi
        };

    public static DonViTuVanKeHoach ToEntity(this DonViTuVanKeHoachModel model)
        => model.ToEntity(model.KeHoachId);

  
}