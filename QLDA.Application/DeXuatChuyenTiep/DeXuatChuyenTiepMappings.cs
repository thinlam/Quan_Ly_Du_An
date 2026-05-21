using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.DeXuatChuyenTieps.DTOs;

namespace QLDA.Application.DeXuatChuyenTieps;

public static class DeXuatChuyenTiepMappings
{
    public static DeXuatChuyenTiep ToEntity(this DeXuatChuyenTiepInsertDto dto)
    {
        return new DeXuatChuyenTiep
        {
            Id = dto.Id ?? Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            UocGiaiNgan = dto.UocGiaiNgan,
            SoLieuGiaiNgan = dto.SoLieuGiaiNgan,
            KhoiLuongDuKien = dto.KhoiLuongDuKien,
            NhuCauKinhPhi = dto.NhuCauKinhPhi,
            KhoiLuongThucTe = dto.KhoiLuongThucTe
        };
    }

    public static DeXuatChuyenTiepDto ToDto(this DeXuatChuyenTiep entity, List<TepDinhKem>? files = null)
    {
        return new DeXuatChuyenTiepDto
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            UocGiaiNgan = entity.UocGiaiNgan,
            NhuCauKinhPhi = entity.NhuCauKinhPhi,
            SoLieuGiaiNgan = entity.SoLieuGiaiNgan,
            KhoiLuongDuKien = entity.KhoiLuongDuKien,
            KhoiLuongThucTe = entity.KhoiLuongThucTe,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
        };
    }
}