using BuildingBlocks.Domain.Entities.Abstractions;
using QLDA.Application.DeXuatChuTruongMois.DTOs;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.DeXuatChuTruongMois;

public static class DeXuatChuTruongMoiMappingConfiguration {
    public static DeXuatChuTruongMoiModel ToModel(this DeXuatChuTruongMoi entity, List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            TongMucDauTu = entity.TongMucDauTu,
            TomTatNoiDung = entity.TomTatNoiDung,
            NgayBatDauDuKien = entity.NgayBatDauDuKien,
            LanhDaoPhuTrachId = entity.LanhDaoPhuTrachId,
            NguoiXuLyChinhId = entity.NguoiXuLyChinhId,
            DonViPhuTrachChinhId = entity.DonViPhuTrachChinhId,
            HinhThucDauTuId = entity.HinhThucDauTuId,
            TrangThaiId = entity.TrangThaiId,
            DonViPhoiHopIds = entity.DeXuatDonViXuLys?.Select(x => x.RightId).ToList(),
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList()
        };


    public static DeXuatChuTruongMoi ToEntity(this DeXuatChuTruongMoiModel model) =>
        new() {
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TongMucDauTu = model.TongMucDauTu,
            TomTatNoiDung = model.TomTatNoiDung,
            NgayBatDauDuKien = model.NgayBatDauDuKien,
            LanhDaoPhuTrachId = model.LanhDaoPhuTrachId,
            NguoiXuLyChinhId = model.NguoiXuLyChinhId,
            HinhThucDauTuId = model.HinhThucDauTuId,
            DonViPhuTrachChinhId = model.DonViPhuTrachChinhId,
            DeXuatDonViXuLys = model.DonViPhoiHopIds?
                .Select(donViId => new DeXuatDonViXuLy { RightId = donViId })
                .ToList() ?? [],
        };

    public static DeXuatChuTruongMoiInsertDto ToInsertDto(this DeXuatChuTruongMoiModel model) =>
        new() {
            Id = model.GetId(),
            DuAnId = model.DuAnId,
            BuocId = model.BuocId,
            TomTatNoiDung = model.TomTatNoiDung,
            TongMucDauTu = model.TongMucDauTu,
            NgayBatDauDuKien = model.NgayBatDauDuKien,
            HinhThucDauTuId = model.HinhThucDauTuId,
            LanhDaoPhuTrachId = model.LanhDaoPhuTrachId,
            NguoiXuLyChinhId = model.NguoiXuLyChinhId,
            DonViPhuTrachChinhId = model.DonViPhuTrachChinhId,
            TrangThaiId = model.TrangThaiId,
            DonViPhoiHopIds = model.DonViPhoiHopIds,
        };

    public static void Update(this DeXuatChuTruongMoi entity, DeXuatChuTruongMoiModel model) {
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.TongMucDauTu = model.TongMucDauTu;
        entity.TomTatNoiDung = model.TomTatNoiDung;
        entity.NgayBatDauDuKien = model.NgayBatDauDuKien;
        entity.LanhDaoPhuTrachId = model.LanhDaoPhuTrachId;
        entity.NguoiXuLyChinhId = model.NguoiXuLyChinhId;
        entity.HinhThucDauTuId = model.HinhThucDauTuId;
        entity.DonViPhuTrachChinhId = model.DonViPhuTrachChinhId;
    }
}