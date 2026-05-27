using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhPheDuyets.DTOs;

namespace QLDA.Application.ToTrinhPheDuyets;

public static class ToTrinhPheDuyetMappings {
    public static ToTrinhPheDuyet ToEntity(this ToTrinhPheDuyetInsertDto dto) {
        return new ToTrinhPheDuyet {
            Id = dto.Id ?? Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            NgayToTrinh = dto.NgayToTrinh,
            TrichYeu = dto.TrichYeu,
            So = dto.So
        };
    }

    public static ToTrinhPheDuyet ToEntity(this ToTrinhPheDuyetUpdateDto dto) {
        return new ToTrinhPheDuyet {
            Id = dto.Id,
            NgayToTrinh = dto.NgayToTrinh.ToStartOfDayUtc(),
            TrichYeu = dto.TrichYeu,
            So = dto.So
        };
    }

    public static ToTrinhPheDuyetDto ToDto(this ToTrinhPheDuyet entity, List<TepDinhKem>? files = null) {
        return new ToTrinhPheDuyetDto
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NgayToTrinh = entity.NgayToTrinh,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            Loai = entity.Loai,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
        };
    }
}