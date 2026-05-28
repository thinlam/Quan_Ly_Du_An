using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;
namespace QLDA.WebApi.Models.ToTrinhThamDinhNhaThaus;

public static class ToTrinhThamDinhNhaThauMappingConfiguration
{
    public static ToTrinhThamDinhNhaThauModel ToModel(this ToTrinhThamDinhNhaThau entity, List<TepDinhKem>? danhSachTepDinhKem = null, List<TepDinhKem> ? danhSachTepThamDinh = null) =>
        new()
        {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            NgayTrinh = entity.NgayTrinh,
            DanhSachNhaThaus = entity.NhaThaus?.Select(x => x.ToModel()).ToList(),
            TrangThaiDangTaiId = entity.TrangThaiDangTaiId,
            DaThamDinh = entity.DaThamDinh,
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList(),
            DanhSachTepThamDinh = danhSachTepThamDinh?.Select(o => o.ToModel()).ToList(),
        };


    public static ToTrinhThamDinhNhaThau ToEntity(this ToTrinhThamDinhNhaThauModel model)
        => new()
        {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TrichYeu = model.TrichYeu,
            So = model.So,
            NgayTrinh = model.NgayTrinh,
            TrangThaiDangTaiId = model.TrangThaiDangTaiId,
            NhaThaus = model.DanhSachNhaThaus?.Select(x => x.ToEntity()).ToList() ?? [],
            DaThamDinh = model.DaThamDinh,
        };
 
}