using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.KeHoachLuaChonNhaThauRutGons;

public static class KetQuaLuaChonNhaThauRutGonMappingConfiguration
{
    public static KeHoachLuaChonNhaThauRutGonModel ToModel(this KeHoachLuaChonNhaThauRutGon entity, List<Attachment>? danhSachTepDinhKem = null) =>
        new()
        {
            Id = entity.Id,
            NhaThauId = entity.NhaThauId,
            GoiThauId = entity.GoiThauId,
            KetQuaDanhGia = entity.KetQuaDanhGia,
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList(),
        };


    public static KeHoachLuaChonNhaThauRutGon ToEntity(this KeHoachLuaChonNhaThauRutGonModel model)
        => new()
        {
            Id = model.GetId(),
            NhaThauId = model.NhaThauId,
            GoiThauId = model.GoiThauId,
            KetQuaDanhGia = model.KetQuaDanhGia
        };

   
}
