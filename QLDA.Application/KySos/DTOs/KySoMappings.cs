using QLDA.Domain.Entities;

namespace QLDA.Application.KySos.DTOs;

public static class KySoMappings {
    public static KySo ToEntity(this KySoInsertDto dto) => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        ChuSoHuuId = dto.ChuSoHuuId,
        Email = dto.Email,
        ChucVuId = dto.ChucVuId,
        PhamVi = dto.PhamVi,
        PhongBanId = dto.PhongBanId,
        SerialChungThu = dto.SerialChungThu,
        ToChucCap = dto.ToChucCap,
        HieuLucTu = dto.HieuLucTu,
        HieuLucDen = dto.HieuLucDen,
        PhuongThucKySoId = dto.PhuongThucKySoId
    };

    public static void Update (this KySo entity, KySoUpdateModel dto)
    {
        entity.ChuSoHuuId = dto.ChuSoHuuId;
        entity.Email = dto.Email;
        entity.ChucVuId = dto.ChucVuId;
        entity.PhamVi = dto.PhamVi;
        entity.PhongBanId = dto.PhongBanId;
        entity.SerialChungThu = dto.SerialChungThu;
        entity.ToChucCap = dto.ToChucCap;
        entity.HieuLucTu = dto.HieuLucTu;
        entity.HieuLucDen = dto.HieuLucDen;
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
        HieuLucTu = entity.HieuLucTu,
        HieuLucDen = entity.HieuLucDen,
        PhuongThucKySoId = entity.PhuongThucKySoId,
        TenPhuongThucKySo = entity.PhuongThucKySo?.Ten
    };
}