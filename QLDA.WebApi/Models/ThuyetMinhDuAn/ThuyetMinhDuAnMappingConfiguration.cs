using BuildingBlocks.Domain.Entities.Abstractions;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using static Dapper.SqlMapper;

namespace QLDA.WebApi.Models.ThuyetMinhDuAns;

public static class ThuyetMinhDuAnMappingConfiguration
{
    public static ThuyetMinhDuAnModel ToModel(this ThuyetMinhDuAn entity, List<TepDinhKem>? danhSachTepDinhKem = null, List<TepDinhKem>? danhSachTepThamDinh = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            So = entity.So,
            NgayTrinh = entity.NgayTrinh,
            TrichYeu = entity.TrichYeu,
            KetQuaThamDinh = entity.KetQuaThamDinh,
            TrangThaiThamDinhId = entity.TrangThaiThamDinhId,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList(),
            DanhSachTepThamDinh = danhSachTepThamDinh?.Select(o => o.ToModel()).ToList()
        };


    public static ThuyetMinhDuAn ToEntity(this ThuyetMinhDuAnModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            So = model.So,
            NgayTrinh = model.NgayTrinh,
            TrichYeu = model.TrichYeu,
            KetQuaThamDinh = model.KetQuaThamDinh,
            TrangThaiThamDinhId = model.TrangThaiThamDinhId,
            TrangThaiId = model.TrangThaiId
        };

    public static void Update(this ThuyetMinhDuAn entity, ThuyetMinhDuAnModel model) {
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.So = model.So;
        entity.NgayTrinh = model.NgayTrinh;
        entity.TrichYeu = model.TrichYeu;
        entity.KetQuaThamDinh = model.KetQuaThamDinh;
        entity.TrangThaiThamDinhId = model.TrangThaiThamDinhId;
     
    }
}