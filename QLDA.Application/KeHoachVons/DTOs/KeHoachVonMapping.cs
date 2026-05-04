using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.KeHoachVons.DTOs;

public static class KeHoachVonMapping {

    public static KeHoachVon ToEntity(this KeHoachVonInsertModel model, Guid duAnId) {
        return new KeHoachVon {
            Id = GuidExtensions.GetSequentialGuidId(),
            DuAnId = duAnId,
            NguonVonId = model.NguonVonId,
            Nam = model.Nam,
            SoVon = model.SoVon,
            SoVonDieuChinh = model.SoVonDieuChinh,
            SoQuyetDinh = model.SoQuyetDinh,
            NgayKy = model.NgayKy,
            GhiChu = model.GhiChu
        };
    }

    public static KeHoachVon ToEntity(this KeHoachVonUpdateModel model, Guid duAnId) {
        return new KeHoachVon {
            Id = model.Id.GetId(),
            DuAnId = duAnId,
            NguonVonId = model.NguonVonId,
            Nam = model.Nam,
            SoVon = model.SoVon,
            SoVonDieuChinh = model.SoVonDieuChinh,
            SoQuyetDinh = model.SoQuyetDinh,
            NgayKy = model.NgayKy,
            GhiChu = model.GhiChu
        };
    }

    /// <summary>
    /// Tạo entity KeHoachVon kèm danh sách tệp đính kèm (dùng cho update)
    /// </summary>
    public static (KeHoachVon, List<TepDinhKem>) ToEntityWithFiles(this KeHoachVonUpdateModel model, Guid duAnId) {
        var khv = model.ToEntity(duAnId);
        var files = model.DanhSachTepDinhKem?.ToEntities(
            groupId: khv.Id,
            groupType: EGroupType.KeHoachVon
        ) ?? [];
        return (khv, [.. files]);
    }

    public static KeHoachVonDto ToDto(this KeHoachVon entity) {
        return new KeHoachVonDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            NguonVonId = entity.NguonVonId,
            Nam = entity.Nam,
            SoVon = entity.SoVon,
            SoVonDieuChinh = entity.SoVonDieuChinh,
            SoQuyetDinh = entity.SoQuyetDinh,
            NgayKy = entity.NgayKy,
            GhiChu = entity.GhiChu
        };
    }
}
