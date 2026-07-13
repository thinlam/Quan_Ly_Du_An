using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.ThanhLyHopDongs.DTOs;

public static class ThanhLyHopDongsMappings {
    public static ThanhLyHopDong ToEntity(this ThanhLyHopDongInsertDto dto) {
        return new ThanhLyHopDong {
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            HopDongId = dto.HopDongId,
            So = dto.So,
            Ngay = dto.Ngay,
            TrichYeu = dto.TrichYeu,
        };
    }

    public static ThanhLyHopDong ToEntity(this ThanhLyHopDongUpdateDto dto) {
        return new ThanhLyHopDong {
            Id = dto.Id,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            HopDongId = dto.HopDongId,
            So = dto.So,
            Ngay = dto.Ngay,
            TrichYeu = dto.TrichYeu,
        };
    }

    public static ThanhLyHopDongDto ToDto(this ThanhLyHopDong entity, IEnumerable<TepDinhKem>? files = null) {
        return new ThanhLyHopDongDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            HopDongId = entity.HopDongId,
            HopDongTen = entity.HopDong?.SoHopDong,
            So = entity.So,
            Ngay = entity.Ngay,
            TrichYeu = entity.TrichYeu,
            TrangThaiId = entity.TrangThaiId,
            TrangThaiTen = entity.TrangThai?.Ten,
            NghiemThuIds = entity.DanhSachNghiemThus?.Select(j => j.RightId).ToList(),
            BienBanNghiemThus = [.. files?.Where(o => o.GroupType == nameof(EGroupType.ThanhLyHopDong_BienBanNghiemThu)).ToDtos() ?? []],
            ThanhLyHopDongs = [.. files?.Where(o => o.GroupType == nameof(EGroupType.ThanhLyHopDong)).ToDtos() ?? []],
            Khacs = [.. files?.Where(o => o.GroupType == nameof(EGroupType.ThanhLyHopDong_Khac)).ToDtos() ?? []]
        };
    }

    public static void Update(this ThanhLyHopDong entity, ThanhLyHopDongUpdateDto dto) {
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        entity.HopDongId = dto.HopDongId;
        entity.So = dto.So;
        entity.Ngay = dto.Ngay;
        entity.TrichYeu = dto.TrichYeu;
    }
}
