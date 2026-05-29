using QLDA.WebApi.Models.DonViTuVanKeHoachs;
using QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.DonViTuVanKeHoachs;

public static class DonViTuVanKeHoachMappingConfiguration {
    public static DonViTuVanKeHoachModel ToModel(this DonViTuVanKeHoach entity) =>
        new() {
            Id = entity.Id,
            KeHoachId = entity.KeHoachId,
            TenDonVi = entity.TenDonVi
        };


    public static DonViTuVanKeHoach ToEntity(this DonViTuVanKeHoachModel model)
        => new() {
            Id = model.GetId(),
            KeHoachId = model.KeHoachId,    
            TenDonVi = model.TenDonVi   
        };

  
}