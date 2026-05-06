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
}