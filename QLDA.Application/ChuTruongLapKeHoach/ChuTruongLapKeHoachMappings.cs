using QLDA.Application.ChuTruongLapKeHoachs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ChuTruongLapKeHoachs;

public static class ChuTruongLapKeHoachMappings
{
    public static ChuTruongLapKeHoach ToEntity(this ChuTruongLapKeHoachDto dto) {
        return new ChuTruongLapKeHoach {
            Id = dto.Id?? Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            TrangThaiId = dto.TrangThaiId,
            SoToTrinh = dto.SoToTrinh,
            NgayToTrinh = dto.NgayToTrinh,
            TrichYeu = dto.TrichYeu,
            ButPhe = dto.ButPhe,
            LoaiDeXuat = dto.LoaiDeXuat,
        };
    }
    public static ChuTruongLapKeHoachDto ToDto(this ChuTruongLapKeHoach entity, List<Attachment> tepDinhKems) {
        return new ChuTruongLapKeHoachDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            SoToTrinh = entity.SoToTrinh,
            NgayToTrinh = entity.NgayToTrinh,
            TrichYeu = entity.TrichYeu,
            ButPhe = entity.ButPhe,
            LoaiDeXuat = entity.LoaiDeXuat,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = tepDinhKems?.Select(o => o.ToDto()).ToList(),
        };
    }
}