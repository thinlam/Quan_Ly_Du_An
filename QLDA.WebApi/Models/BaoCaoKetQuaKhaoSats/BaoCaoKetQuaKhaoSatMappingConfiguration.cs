using BuildingBlocks.CrossCutting.ExtensionMethods;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.WebApi.Models.BaoCaoKetQuaKhaoSats;

public static class BaoCaoKetQuaKhaoSatMappingConfiguration
{
    public static BaoCaoKetQuaKhaoSatModel ToModel(this BaoCaoKetQuaKhaoSat entity) => new()
    {
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        NoiDungBaoCao = entity.NoiDungBaoCao,
        NoiDungNghiemThu = entity.NoiDungNghiemThu,
        NgayKhaoSat = entity.NgayKhaoSat.ToDateOnlyVn(),
    };

    public static BaoCaoKetQuaKhaoSatInsertDto ToInsertDto(this BaoCaoKetQuaKhaoSatModel model) => new()
    {
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        NoiDungBaoCao = model.NoiDungBaoCao,
        NoiDungNghiemThu = model.NoiDungNghiemThu,
        NgayKhaoSat = model.NgayKhaoSat,
    };

    public static BaoCaoKetQuaKhaoSatUpdateModel ToUpdateModel(this BaoCaoKetQuaKhaoSatModel model) => new()
    {
        Id = model.GetId(),
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        NoiDungBaoCao = model.NoiDungBaoCao,
        NoiDungNghiemThu = model.NoiDungNghiemThu,
        NgayKhaoSat = model.NgayKhaoSat,
    };
}
