using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;

public static class KetQuaThamDinhNhaThauMappingConfiguration
{
    public static KetQuaThamDinhNhaThauModel ToModel(this KetQuaThamDinhNhaThau entity, List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new()
        {
            Id = entity.Id,
            ToTrinhId = entity.ToTrinhId,
            NhaThauId = entity.NhaThauId,
            GoiThauId = entity.GoiThauId,
            KetQuaDanhGia = entity.KetQuaDanhGia,
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList(),
        };


    public static KetQuaThamDinhNhaThau ToEntity(this KetQuaThamDinhNhaThauModel model)
        => new()
        {
            //Id = model.GetId(),
            ToTrinhId = model.ToTrinhId,
            NhaThauId = model.NhaThauId,
            GoiThauId = model.GoiThauId,
            KetQuaDanhGia = model.KetQuaDanhGia
        };
 
}