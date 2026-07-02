using BuildingBlocks.CrossCutting.ExtensionMethods;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;

public static class HoSoDeXuatCapDoCnttMappings
{
    public static HoSoDeXuatCapDoCntt ToEntity(this HoSoDeXuatCapDoCnttInsertDto dto) => new()
    {
        Id = Guid.NewGuid(),
        DuAnId = dto.DuAnId,
        BuocId = dto.BuocId,
        TrangThaiId = dto.TrangThaiId,
        CapDoId = dto.CapDoId,
        NgayTrinh = dto.NgayTrinh.ToStartOfDayUtc(),
        DonViChuTriId = dto.DonViChuTriId,
        NoiDungDeNghi = dto.NoiDungDeNghi,
        NoiDungBaoCao = dto.NoiDungBaoCao,
        NoiDungDuThao = dto.NoiDungDuThao,
    };

    public static void Update (this HoSoDeXuatCapDoCntt entity, HoSoDeXuatCapDoCnttUpdateModel model)
    {
      //  entity.TrangThaiId = model.TrangThaiId;
        entity.CapDoId = model.CapDoId;
        entity.NgayTrinh = model.NgayTrinh.HasValue
            ? model.NgayTrinh.ToStartOfDayUtc()
            : DateOnly.FromDateTime(DateTime.UtcNow).ToStartOfDayUtc();
        entity.DonViChuTriId = model.DonViChuTriId;
        entity.NoiDungDeNghi = model.NoiDungDeNghi;
        entity.NoiDungBaoCao = model.NoiDungBaoCao;
        entity.NoiDungDuThao = model.NoiDungDuThao;

    }
    public static HoSoDeXuatCapDoCnttDto ToDto (this HoSoDeXuatCapDoCntt entity) => new()
    {
      Id = entity.Id,
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        TrangThaiId = entity.TrangThaiId,
        MaTrangThai = entity.TrangThai?.Ma,
        CapDoId = entity.CapDoId,
        TenCapDo = entity.CapDo?.Ten,
        NgayTrinh = entity.NgayTrinh.ToDateOnlyVn(),
        DonViChuTriId = entity.DonViChuTriId,
        NoiDungDeNghi = entity.NoiDungDeNghi,
        NoiDungBaoCao = entity.NoiDungBaoCao,
        NoiDungDuThao = entity.NoiDungDuThao,
        TenTrangThai = entity.TrangThaiId == null ? TrangThaiPheDuyetCodes.Default.TenDuThao : entity.TrangThai?.Ten,
    };
}
