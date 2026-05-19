using QLDA.Domain.Entities;
using QLDA.WebApi.Models.HoSoDeXuatCapDoCntts;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.ToTrinhKeHoachs;

public static class ToTrinhKeHoachMappingConfiguration {
    public static ToTrinhKeHoachModel ToModel(this ToTrinhKeHoach entity,  List<TepDinhKem>? files = null) => new()
       {

           Id = entity.Id,
           DuAnId = entity.DuAnId,
           BuocId = entity.BuocId,
           TrangThaiId = entity.TrangThaiId,
           DanhSachTepDinhKem = files?.Select(o => new TepDinhKemModel
           {
               Id = o.Id,
               ParentId = o.ParentId,
               GroupId = o.GroupId,
               GroupType = o.GroupType,
               Path = o.Path,
               Size = o.Size,
               Type = o.Type,
               FileName = o.FileName,
               OriginalName = o.OriginalName
           }).ToList()
       };
    public static ToTrinhKeHoach ToEntity(this ToTrinhKeHoachModel model) => new() {
        Id = model.GetId(),
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        So = model.SoToTrinh,
        NgayToTrinh = model.NgayToTrinh,
        TrichYeu = model.TrichYeu,
    };

    public static void UpdateFrom(this ToTrinhKeHoach entity, ToTrinhKeHoachModel model) {
        entity.DuAnId = model.DuAnId;
        entity.BuocId = model.BuocId;
        entity.So = model.SoToTrinh;
        entity.NgayToTrinh = model.NgayToTrinh;
        entity.TrichYeu = model.TrichYeu;
    }
}