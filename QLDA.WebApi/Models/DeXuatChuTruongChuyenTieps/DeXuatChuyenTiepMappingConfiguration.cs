using BuildingBlocks.Domain.Entities.Abstractions;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.DeXuatChuTruongChuyenTieps;

public static class DeXuatChuyenTiepMappingConfiguration {
    public static DeXuatChuyenTiepModel ToModel(this DeXuatChuyenTiep entity, List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            NhuCauKinhPhi = entity.NhuCauKinhPhi,
            SoLieuGiaiNgan = entity.SoLieuGiaiNgan,
            UocGiaiNgan = entity.UocGiaiNgan,
            KhoiLuongThucTe = entity.KhoiLuongThucTe,
            KhoiLuongDuKien = entity.KhoiLuongDuKien,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList()
        };


    public static DeXuatChuyenTiep ToEntity(this DeXuatChuyenTiepModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            NhuCauKinhPhi = model.NhuCauKinhPhi,
            SoLieuGiaiNgan = model.SoLieuGiaiNgan,
            UocGiaiNgan = model.UocGiaiNgan,
            KhoiLuongThucTe = model.KhoiLuongDuKien,
            KhoiLuongDuKien = model.KhoiLuongDuKien

        };

    public static void Update(this DeXuatChuyenTiep entity, DeXuatChuyenTiepModel model) {
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.NhuCauKinhPhi = model.NhuCauKinhPhi;
        entity.SoLieuGiaiNgan = model.SoLieuGiaiNgan;
        entity.UocGiaiNgan = model.UocGiaiNgan;
        entity.KhoiLuongThucTe = model.KhoiLuongThucTe;
        entity.KhoiLuongDuKien = model.KhoiLuongDuKien;

    }
}