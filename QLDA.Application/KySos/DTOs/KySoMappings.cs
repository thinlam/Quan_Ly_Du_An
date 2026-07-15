using QLDA.Domain.Enums;

namespace QLDA.Application.KySos.DTOs;

public static class KySoMappings {
    public static KySo ToEntity(this KySoInsertDto dto) => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        ChuSoHuuId = dto.ChuSoHuuId,
        Email = dto.Email,
        ChucVuId = dto.ChucVuId,
        PhamVi = dto.PhamVi.HasValue ? (EPhamViKySo)dto.PhamVi : null,
        PhongBanId = dto.PhongBanId,
        SerialChungThu = dto.SerialChungThu,
        ToChucCap = dto.ToChucCap,
        HieuLucTu = dto.HieuLucTu.ToStartOfDayUtc(),
        HieuLucDen = dto.HieuLucDen.ToStartOfDayUtc(),
        PhuongThucKySoId = dto.PhuongThucKySoId
    };

    public static void Update(this KySo entity, KySoUpdateModel dto) {
        entity.ChuSoHuuId = dto.ChuSoHuuId;
        entity.Email = dto.Email;
        entity.ChucVuId = dto.ChucVuId;
        entity.PhamVi = dto.PhamVi.HasValue ? (EPhamViKySo)dto.PhamVi : null;
        entity.PhongBanId = dto.PhongBanId;
        entity.SerialChungThu = dto.SerialChungThu;
        entity.ToChucCap = dto.ToChucCap;
        entity.HieuLucTu = dto.HieuLucTu.ToStartOfDayUtc();
        entity.HieuLucDen = dto.HieuLucDen.ToStartOfDayUtc();
        entity.PhuongThucKySoId = dto.PhuongThucKySoId;
    }

    public static KySoDto ToDto(this KySo entity) => new() {
        Id = entity.Id,
        ChuSoHuuId = entity.ChuSoHuuId,
        Email = entity.Email,
        ChucVuId = entity.ChucVuId,
        TenChucVu = entity.ChucVu?.Ten,
        PhamVi = entity.PhamVi,
        PhongBanId = entity.PhongBanId,
        SerialChungThu = entity.SerialChungThu,
        ToChucCap = entity.ToChucCap,
        HieuLucTu = entity.HieuLucTu.ToDateOnlyVn(),
        HieuLucDen = entity.HieuLucDen.ToDateOnlyVn(),
        PhuongThucKySoId = entity.PhuongThucKySoId,
        TenPhuongThucKySo = entity.PhuongThucKySo?.Ten
    };
}
