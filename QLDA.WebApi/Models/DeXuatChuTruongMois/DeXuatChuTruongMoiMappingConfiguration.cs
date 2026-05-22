using BuildingBlocks.Domain.Entities.Abstractions;
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
            NguoiXuLyChinhId = entity.LanhDaoPhuTrachId,
            DonViPhuTrachChinhId = entity.DonViPhuTrachChinhId,
            HinhThucDauTuId = entity.HinhThucDauTuId,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList()
        };


    public static DeXuatChuTruongMoi ToEntity(this DeXuatChuTruongMoiModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TongMucDauTu = model.TongMucDauTu,
            TomTatNoiDung = model.TomTatNoiDung,
            NgayBatDauDuKien = model.NgayBatDauDuKien,
            LanhDaoPhuTrachId = model.LanhDaoPhuTrachId,
            NguoiXuLyChinhId = model.NguoiXuLyChinhId,
            HinhThucDauTuId = model.HinhThucDauTuId,
            DonViPhuTrachChinhId = model.DonViPhuTrachChinhId

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