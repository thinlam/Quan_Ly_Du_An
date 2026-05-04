using QLDA.Application.PheDuyetDuToans.DTOs;

namespace QLDA.Application.PheDuyetDuToans;

public static class PheDuyetDuToanMappings {
    public static PheDuyetDuToan ToEntity(this PheDuyetDuToanInsertDto dto) {
        return new PheDuyetDuToan {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.SoVanBan,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy,
            ChucVuId = dto.ChucVuId,
            GiaTriDuThau = dto.GiaTriDuThau,
            TrichYeu = dto.TrichYeu
        };
    }

    public static PheDuyetDuToan ToEntity(this PheDuyetDuToanUpdateDto dto) {
        return new PheDuyetDuToan {
            Id = dto.Id,
            So = dto.SoVanBan,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy,
            ChucVuId = dto.ChucVuId,
            GiaTriDuThau = dto.GiaTriDuThau,
            TrichYeu = dto.TrichYeu
        };
    }

    public static PheDuyetDuToanDto ToDto(this PheDuyetDuToan entity) {
        return new PheDuyetDuToanDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            SoVanBan = entity.So,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy,
            ChucVuId = entity.ChucVuId,
            GiaTriDuThau = entity.GiaTriDuThau,
            TrichYeu = entity.TrichYeu,
            TrangThaiId = entity.TrangThaiId,
            NguoiXuLyId = entity.NguoiXuLyId,
            NguoiGiaoViecId = entity.NguoiGiaoViecId
        };
    }
}