using QLDA.Application.ToTrinhKeHoachs.DTOs;

namespace QLDA.Application.ToTrinhKeHoachs;

public static class ToTrinhKeHoachMappings {
    public static ToTrinhKeHoach ToEntity(this ToTrinhKeHoachInsertDto dto) {
        return new ToTrinhKeHoach {
            Id = dto.Id ?? Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            NgayToTrinh = dto.NgayToTrinh,
            TrichYeu = dto.TrichYeu,
            So = dto.So
        };
    }

    public static ToTrinhKeHoach ToEntity(this ToTrinhKeHoachUpdateDto dto) {
     
        return new ToTrinhKeHoach {
            Id = dto.Id,
            NgayToTrinh = dto.NgayToTrinh,
            TrichYeu = dto.TrichYeu,
            So = dto.So
        };
    }

    public static ToTrinhKeHoachDto ToDto(this ToTrinhKeHoach entity) {
        return new ToTrinhKeHoachDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NgayToTrinh = entity.NgayToTrinh,
            TrichYeu = entity.TrichYeu,
            So = entity.So 
,
        };
    }
}