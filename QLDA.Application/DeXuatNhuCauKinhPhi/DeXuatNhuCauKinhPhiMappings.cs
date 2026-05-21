using Azure.Core;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.DeXuatNhuCauKinhPhiMappings;

public static class DeXuatNhuCauKinhPhiMappings
{
    public static DeXuatNhuCauKinhPhi ToEntity(this DeXuatNhuCauKinhPhiInsertDto dto) {
        return new DeXuatNhuCauKinhPhi {
            Id = dto.Id ?? Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            DonViDeXuatId = dto.DonViDeXuatId,
            SoPhieuChuyen = dto.SoPhieuChuyen,
            NgayPhieuChuyen = dto.NgayPhieuChuyen,
            TrichYeu = dto.TrichYeu
        };
    }

    public static DeXuatNhuCauKinhPhiDto ToDto(this DeXuatNhuCauKinhPhi entity, List<TepDinhKem>? files = null) {
        return new DeXuatNhuCauKinhPhiDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            DonViDeXuatId = entity.DonViDeXuatId,
            SoPhieuChuyen = entity.SoPhieuChuyen,
            NgayPhieuChuyen = entity.NgayPhieuChuyen,
            TrichYeu = entity.TrichYeu,
            KinhPhiDeXuat = entity.KinhPhiDeXuat,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
        };
    }
}