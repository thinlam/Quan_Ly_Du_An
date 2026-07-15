using QLDA.Application.PhuLucHopDongs.DTOs;

namespace QLDA.Application.PhuLucHopDongs;

public static class PhuLucHopDongMappings {
    public static PhuLucHopDong ToEntity(this PhuLucHopDongInsertDto dto) {
        return new PhuLucHopDong {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            Ten = dto.Ten,
            SoPhuLucHopDong = dto.SoPhuLucHopDong,
            NoiDung = dto.NoiDung,
            Ngay = dto.Ngay,
            HopDongId = dto.HopDongId,
            GiaTri = dto.GiaTri,
            NgayDuKienKetThuc = dto.NgayDuKienKetThuc
        };
    }

    public static PhuLucHopDong ToEntity(this PhuLucHopDongUpdateDto dto) {
        return new PhuLucHopDong {
            Id = dto.Id,
            Ten = dto.Ten,
            SoPhuLucHopDong = dto.SoPhuLucHopDong,
            NoiDung = dto.NoiDung,
            Ngay = dto.Ngay,
            HopDongId = dto.HopDongId,
            GiaTri = dto.GiaTri,
            NgayDuKienKetThuc = dto.NgayDuKienKetThuc
        };
    }

    public static PhuLucHopDongDto ToDto(this PhuLucHopDong entity) {
        return new PhuLucHopDongDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            Ten = entity.Ten ?? string.Empty,
            SoPhuLucHopDong = entity.SoPhuLucHopDong,
            NoiDung = entity.NoiDung,
            Ngay = entity.Ngay,
            HopDongId = entity.HopDongId,
            GiaTri = entity.GiaTri,
            NgayDuKienKetThuc = entity.NgayDuKienKetThuc
        };
    }
}