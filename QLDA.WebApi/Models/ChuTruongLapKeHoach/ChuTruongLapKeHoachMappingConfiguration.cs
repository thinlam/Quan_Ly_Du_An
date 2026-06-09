using QLDA.Application.ChuTruongLapKeHoachs.DTOs;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.ChuTruongLapKeHoachs;

public static class ChuTruongLapKeHoachMappingConfiguration {
    public static ChuTruongLapKeHoachModel ToModel(this ChuTruongLapKeHoach entity,
        List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new() {
            Id =  entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            TrichYeu = entity.TrichYeu,
            SoToTrinh = entity.SoToTrinh,
            NgayToTrinh = entity.NgayToTrinh,
            LoaiDeXuat = entity.LoaiDeXuat,
            ButPhe = entity.ButPhe,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                .Select(o => o.ToModel()).ToList()
        };


    public static ChuTruongLapKeHoach ToEntity(this ChuTruongLapKeHoachModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TrichYeu = model.TrichYeu,
            SoToTrinh = model.SoToTrinh,
            LoaiDeXuat = model.LoaiDeXuat,
            NgayToTrinh = model.NgayToTrinh,
            ButPhe = model.ButPhe,
        };
    public static ChuTruongLapKeHoachDto ToDto(this ChuTruongLapKeHoach model)
      => new()
      {
          Id = model.Id,
          BuocId = model.BuocId,
          DuAnId = model.DuAnId,
          TrichYeu = model.TrichYeu,
          SoToTrinh = model.SoToTrinh,
          LoaiDeXuat = model.LoaiDeXuat,
          NgayToTrinh = model.NgayToTrinh,
          ButPhe = model.ButPhe,
      };

    public static void Update(this ChuTruongLapKeHoach entity, ChuTruongLapKeHoachModel model) {
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.TrichYeu = model.TrichYeu;
        entity.LoaiDeXuat = model.LoaiDeXuat;
        entity.SoToTrinh = model.SoToTrinh;
        entity.NgayToTrinh = model.NgayToTrinh;
        entity.ButPhe = model.ButPhe;
    }
}