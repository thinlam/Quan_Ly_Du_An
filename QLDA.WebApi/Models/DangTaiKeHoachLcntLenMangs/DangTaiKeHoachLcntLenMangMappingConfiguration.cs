using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.DangTaiKeHoachLcntLenMangs;

public static class DangTaiKeHoachLcntLenMangMappingConfiguration {
    public static DangTaiKeHoachLcntLenMangModel ToModel(this DangTaiKeHoachLcntLenMang entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            KeHoachLuaChonNhaThauId = entity.KeHoachLuaChonNhaThauId,
            NgayEHSMT = entity.NgayEHSMT,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.DangTaiKeHoachLcntLenMang))
                .Select(o => o.ToModel()).ToList()
        };


    public static DangTaiKeHoachLcntLenMang ToEntity(this DangTaiKeHoachLcntLenMangModel model)
        => new() {
            Id = model.GetId(),
            DuAnId = model.DuAnId,
            BuocId = model.BuocId,
            TrangThaiId = model.TrangThaiId,
            NgayEHSMT = model.NgayEHSMT,
            KeHoachLuaChonNhaThauId = model.KeHoachLuaChonNhaThauId,
        };

    public static void Update(this DangTaiKeHoachLcntLenMang entity, DangTaiKeHoachLcntLenMangModel model) {
        entity.DuAnId = model.DuAnId;
        entity.BuocId = model.BuocId;
        entity.TrangThaiId = model.TrangThaiId;
        entity.KeHoachLuaChonNhaThauId = model.KeHoachLuaChonNhaThauId;
        entity.NgayEHSMT = model.NgayEHSMT;
    }
}
