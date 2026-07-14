using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.DeXuatChuTruongChuyenTieps;

public static class DeXuatChuyenTiepMappingConfiguration {
    public static DeXuatChuyenTiepModel ToModel(this DeXuatChuyenTiep entity, List<Attachment>? danhSachTepDinhKem = null) =>
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
            NamDeXuat = entity.NamDeXuat,
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
            KhoiLuongDuKien = model.KhoiLuongDuKien,
            NamDeXuat = model.NamDeXuat,
        };

   
}
