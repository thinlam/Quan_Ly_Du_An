using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
namespace QLDA.WebApi.Models.ToTrinhThamDinhNhaThaus;

public static class ToTrinhThamDinhNhaThauMappingConfiguration
{
    public static ToTrinhThamDinhNhaThauModel ToModel(this ToTrinhThamDinhNhaThau entity,List<KetQuaThamDinhNhaThauModel>? nhaThauModel = null, List<KetQuaThamDinhNhaThauDto>? nhaThaus = null, List<Attachment>? danhSachTepDinhKem = null, List<Attachment> ? danhSachTepThamDinh = null) =>
        new()
        {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            NgayTrinh = entity.NgayTrinh,
            TrangThaiDangTaiId = entity.TrangThaiDangTaiId,
            DaThamDinh = entity.DaThamDinh,
            DanhSachNhaThaus = nhaThauModel ?? new List<KetQuaThamDinhNhaThauModel>(),
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
