using BuildingBlocks.Domain.Entities.Abstractions;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using static Dapper.SqlMapper;

namespace QLDA.WebApi.Models.DeXuatNhuCauKinhPhiNams;

public static class DeXuatNhuCauKinhPhiNamMappingConfiguration
{
    public static DeXuatNhuCauKinhPhiNamModel ToModel(this DeXuatNhuCauKinhPhiNam entity, List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            So = entity.So,
            NgayKeHoach = entity.NgayKeHoach,
            TrichYeu = entity.TrichYeu,
            GhiChu = entity.GhiChu,
            TongKinhPhiDeXuat = entity.TongKinhPhiDeXuat,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList()
        };


    public static DeXuatNhuCauKinhPhiNam ToEntity(this DeXuatNhuCauKinhPhiNamModel model)
        => new() {
            Id = model.GetId(),
            So = model.So,
            NgayKeHoach = model.NgayKeHoach,
            TrichYeu = model.TrichYeu,
            GhiChu = model.GhiChu,
            TongKinhPhiDeXuat = model.TongKinhPhiDeXuat
        };

    //public static void Update(this DeXuatNhuCauKinhPhiNam entity, DeXuatNhuCauKinhPhiNamModel model) {
    //    entity.BuocId = model.BuocId;
    //    entity.DuAnId = model.DuAnId;
    //}
}