using BuildingBlocks.Domain.Entities.Abstractions;
using QLDA.Application.DeXuatChuTruongMois.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.DeXuatChuTruongMois;

public static class DeXuatChuTruongMoiMappings {
    public static DeXuatChuTruongMoi ToEntity(this DeXuatChuTruongMoiInsertDto dto) {
        var id = GuidExtensions.GetSequentialGuidId();
        return new DeXuatChuTruongMoi {
            Id = id,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            NgayBatDauDuKien = dto.NgayBatDauDuKien,
            TomTatNoiDung = dto.TomTatNoiDung,
            DonViPhuTrachChinhId = dto.DonViPhuTrachChinhId,
            HinhThucDauTuId = dto.HinhThucDauTuId,
            LanhDaoPhuTrachId = dto.LanhDaoPhuTrachId,
            NguoiXuLyChinhId = dto.NguoiXuLyChinhId,
            TongMucDauTu = dto.TongMucDauTu,
            DeXuatDonViXuLys = [..dto.DonViPhoiHopIds?.Select(dvPhoiHops => new DeXuatDonViXuLy {
                LeftId = id,
                RightId =dvPhoiHops
            }) ?? []]
        };
    }

    public static DeXuatChuTruongMoiDto ToDto(this DeXuatChuTruongMoi entity, List<TepDinhKem>? files = null) {
        return new DeXuatChuTruongMoiDto
        {
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
            DanhSachDonViPhoiHop = [..entity.DeXuatDonViXuLys?
                                                                .Select(xuLy => new DanhMucDonViCbo
                                                                {
                                                                    Id = xuLy.RightId,
                                                                    TenDonVi =string.Empty // Works if .ThenInclude(x => x.DonVi) was used
                                                                }) ?? []]
        };

    }
}