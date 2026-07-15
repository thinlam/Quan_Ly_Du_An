using QLDA.Application.BaoCaoTienDos.DTOs;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.BaoCaoTienDos;

public static class BaoCaoTienDoMappingConfiguration {
    public static BaoCaoTienDoModel ToModel(this BaoCaoTienDo entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            Ngay = entity.Ngay,
            NoiDung = entity.NoiDung,
            NguoiBaoCaoId = long.Parse(entity.CreatedBy),
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.BaoCaoTienDo))
                .Select(o => o.ToModel()).ToList()
        };


    public static BaoCaoTienDo ToEntity(this BaoCaoTienDoModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId == 0 ? null : model.BuocId,
            DuAnId = model.DuAnId,
            Ngay = model.Ngay,
            NoiDung = model.NoiDung,
        };

    public static void Update(this BaoCaoTienDo entity, BaoCaoTienDoModel model) {
        entity.BuocId = model.BuocId == 0 ? null : model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.Ngay = model.Ngay;
        entity.NoiDung = model.NoiDung;
    }

    public static BaoCaoTienDoImportDto ToImportDto(this BaoCaoTienDoImportModel model)
        => new BaoCaoTienDoImportDto() {
            TenDuAn = model.TenDuAn,
            // TenBuoc = model.TenBuoc,
            NgayBaoCao = model.NgayBaoCao,
            NoiDung = model.NoiDung,
        };

    public static List<BaoCaoTienDoImportDto> ToImportDtoList(this List<BaoCaoTienDoImportModel> entities)
        => [.. entities.Select(e => e.ToImportDto())];
}
