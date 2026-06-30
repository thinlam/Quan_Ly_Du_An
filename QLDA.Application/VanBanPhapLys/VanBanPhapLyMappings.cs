using QLDA.Application.VanBanPhapLys.DTOs;

namespace QLDA.Application.VanBanPhapLys;

public static class VanBanPhapLyMappings {
    public static VanBanPhapLy ToEntity(this VanBanPhapLyInsertDto dto) {
        return new VanBanPhapLy {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.SoVanBan,
            Ngay = dto.NgayVanBan,
            TrichYeu = dto.TrichYeu,
            NgayKy = dto.NgayKy,
            CoQuanQuyetDinh = dto.CoQuanQuyetDinh,
            NguoiKy = dto.NguoiKy,
            LoaiVanBanId = dto.LoaiVanBanId,
            ChucVuId = dto.ChucVuId
        };
    }

    public static VanBanPhapLy ToEntity(this VanBanPhapLyUpdateDto dto) {
        return new VanBanPhapLy {
            Id = dto.Id,
            So = dto.SoVanBan,
            Ngay = dto.NgayVanBan,
            TrichYeu = dto.TrichYeu,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy,
            LoaiVanBanId = dto.LoaiVanBanId,
            CoQuanQuyetDinh = dto.CoQuanQuyetDinh,
            ChucVuId = dto.ChucVuId
        };
    }

    public static VanBanPhapLyDto ToDto(this VanBanPhapLy entity) {
        return new VanBanPhapLyDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            SoVanBan = entity.So,
            NgayVanBan = entity.Ngay,
            TrichYeu = entity.TrichYeu,
            CoQuanQuyetDinh = entity.CoQuanQuyetDinh,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy,
            LoaiVanBanId = entity.LoaiVanBanId,
            ChucVuId = entity.ChucVuId
        };
    }
}