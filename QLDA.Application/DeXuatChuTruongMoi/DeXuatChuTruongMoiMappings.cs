using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.DeXuatChuTruongMois.DTOs;

namespace QLDA.Application.DeXuatChuTruongMois;

public static class DeXuatChuTruongMoiMappings {
    public static DeXuatChuTruongMoi ToEntity(this DeXuatChuTruongMoiInsertDto dto) {
        return new DeXuatChuTruongMoi {
            Id = dto.Id ?? Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            NgayBatDauDuKien = dto.NgayBatDauDuKien,
            TomTatNoiDung = dto.TomTatNoiDung,
            DonViPhuTrachChinhId = dto.DonViPhuTrachChinhId,
            HinhThucDauTuId = dto.HinhThucDauTuId,
            LanhDaoPhuTrachId = dto.LanhDaoPhuTrachId,
            NguoiXuLyChinhId = dto.NguoiXuLyChinhId,
            TongMucDauTu = dto.TongMucDauTu
        };
    }

    public static DeXuatChuTruongMoiDto ToDto(this DeXuatChuTruongMoi entity, List<TepDinhKem>? files = null) {
        return new DeXuatChuTruongMoiDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NgayBatDauDuKien = entity.NgayBatDauDuKien,
            TongMucDauTu = entity.TongMucDauTu,
            TomTatNoiDung = entity.TomTatNoiDung,
            DonViPhuTrachChinhId = entity.DonViPhuTrachChinhId,
            NguoiXuLyChinhId = entity.NguoiXuLyChinhId,
            HinhThucDauTuId = entity.HinhThucDauTuId,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
        };
    }
}