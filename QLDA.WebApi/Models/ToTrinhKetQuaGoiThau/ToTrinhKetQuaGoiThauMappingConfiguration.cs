using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.ToTrinhKetQuaGoiThaus;

public static class ToTrinhKetQuaGoiThauMappingConfiguration
{
    public static ToTrinhKetQuaGoiThauModel ToModel(this ToTrinhKetQuaGoiThau entity,
        List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new()
        {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            NgayTrinh = entity.NgayTrinh,
            TrangThaiDangTaiId = entity.TrangThaiDangTaiId,
            GoiThaus = entity.GoiThaus.Select(o => new GoiThauCboModel
            {
                GoiThauId = o.GoiThauId,
                TenGoiThau = o.GoiThau?.Ten 
            }).ToList() ?? new List<GoiThauCboModel>(),
            DanhSachTepDinhKem = danhSachTepDinhKem?
                .Select(o => o.ToModel()).ToList()
        };


    public static ToTrinhKetQuaGoiThau ToEntity(this ToTrinhKetQuaGoiThauModel model)
        => new()
        {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TrichYeu = model.TrichYeu,
            So = model.So,
            NgayTrinh = model.NgayTrinh,
            TrangThaiDangTaiId = model.TrangThaiDangTaiId,
        };
 
}