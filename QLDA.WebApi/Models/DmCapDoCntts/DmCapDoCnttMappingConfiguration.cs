namespace QLDA.WebApi.Models.DmCapDoCntts;

public static class DmCapDoCnttMappingConfiguration {
    public static DmCapDoCnttModel ToModel(this DmCapDoCntt entity) => new() {
        Id = entity.Id,
        Ma = entity.Ma,
        Ten = entity.Ten,
        MoTa = entity.MoTa,
        Stt = entity.Stt,
        Used = entity.Used,
        MaMau = entity.MaMau
    };

    public static DmCapDoCntt ToEntity(this DmCapDoCnttModel model) => new() {
        Id = model.Id ?? 0,  // For creation: 0 = new entity, EF auto-generates ID; for update: use provided ID
        Ma = model.Ma,
        Ten = model.Ten,
        MoTa = model.MoTa,
        Stt = model.Stt,
        Used = model.Used,
        MaMau = model.MaMau
    };
}