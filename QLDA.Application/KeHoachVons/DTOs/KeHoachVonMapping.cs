
using SharedKernel.CrossCuttingConcerns.ExtensionsMethods;
namespace QLDA.Application.KeHoachVons.DTOs;

public class KeHoachVonMapping {
    public static KeHoachVon ToEntity(this KeHoachVonUpdateDto dto) {
        return new KeHoachVon {
            Id = dto.Id,
            NguonVonId = dto.NguonVonId,
            Nam = dto.Nam,
            SoVon = dto.SoVon,
            SoVonDieuChinh = dto.SoVonDieuChinh,
            SoQuyetDinh = dto.SoQuyetDinh,
            NgayKy = dto.NgayKy,
            GhiChu = dto.GhiChu
        };
    }

    public static KeHoachVon ToEntity(this KeHoachVonUpdateDto dto, Guid duAnId) {
        var entity = dto.ToEntity();
        entity.DuAnId = duAnId;
        return entity;
    }

    public static KeHoachVon ToEntity(this KeHoachVonInsertDto dto) {
        return new KeHoachVon {
            NguonVonId = dto.NguonVonId,
            Nam = dto.Nam,
            SoVon = dto.SoVon,
            SoVonDieuChinh = dto.SoVonDieuChinh,
            SoQuyetDinh = dto.SoQuyetDinh,
            NgayKy = dto.NgayKy,
            GhiChu = dto.GhiChu
        };
    }

    public static KeHoachVon ToEntity(this KeHoachVonInsertDto dto, Guid duAnId) {
        var entity = dto.ToEntity();
        entity.DuAnId = duAnId;
        return entity;
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

    public static void Update(this KeHoachVon entity, KeHoachVonUpdateDto dto) {
        entity.NguonVonId = dto.NguonVonId;
        entity.Nam = dto.Nam;
        entity.SoVon = dto.SoVon;
        entity.SoVonDieuChinh = dto.SoVonDieuChinh;
        entity.SoQuyetDinh = dto.SoQuyetDinh;
        entity.NgayKy = dto.NgayKy;
        entity.GhiChu = dto.GhiChu;
    }
} 
