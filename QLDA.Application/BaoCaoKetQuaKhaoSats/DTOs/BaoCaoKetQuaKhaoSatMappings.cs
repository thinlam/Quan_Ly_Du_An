using BuildingBlocks.CrossCutting.ExtensionMethods;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public static class BaoCaoKetQuaKhaoSatMappings
{
    public static BaoCaoKetQuaKhaoSat ToEntity(this BaoCaoKetQuaKhaoSatInsertDto dto) => new()
    {
        Id = Guid.NewGuid(),
        DuAnId = dto.DuAnId,
        BuocId = dto.BuocId,
        NoiDungBaoCao = dto.NoiDungBaoCao,
        NoiDungNghiemThu = dto.NoiDungNghiemThu,
        NgayKhaoSat = dto.NgayKhaoSat.ToStartOfDayUtc(),
    };

    public static void Update(this BaoCaoKetQuaKhaoSat entity, BaoCaoKetQuaKhaoSatUpdateModel model)
    {
        entity.NoiDungBaoCao = model.NoiDungBaoCao;
        entity.NoiDungNghiemThu = model.NoiDungNghiemThu;
        entity.NgayKhaoSat = model.NgayKhaoSat.ToStartOfDayUtc();
    }

    public static BaoCaoKetQuaKhaoSatDto ToDto(this BaoCaoKetQuaKhaoSat entity) => new()
    {
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        NoiDungBaoCao = entity.NoiDungBaoCao,
        NoiDungNghiemThu = entity.NoiDungNghiemThu,
        NgayKhaoSat = entity.NgayKhaoSat.ToDateOnlyVn(),
        TrangThaiId = entity.TrangThaiId,
        TenTrangThai = entity.TrangThaiId == null
            ? TrangThaiPheDuyetCodes.Default.TenDuThao
            : entity.TrangThai?.Ten,
        NgayTrinh = entity.NgayTrinh.ToDateOnlyVn(),
    };
}
