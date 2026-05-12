using QLDA.Domain.Entities;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.PhanKhaiKinhPhis;

public static class PhanKhaiKinhPhiMappingConfiguration {
    public static PhanKhaiKinhPhiModel ToModel(this PhanKhaiKinhPhi entity) => new() {
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        SoToTrinh = entity.SoToTrinh,
        NgayToTrinh = entity.NgayToTrinh,
        NguonVonId = entity.NguonVonId,
        KinhPhiDeXuat = entity.KinhPhiDeXuat,
        KinhPhiPhanKhai = entity.KinhPhiPhanKhai,
        ThuyetMinh = entity.ThuyetMinh,
        TrangThaiId = entity.TrangThaiId,
        TenTrangThai = entity.TrangThai != null ? entity.TrangThai.Ten : null,
    };

    public static PhanKhaiKinhPhiModel ToModel(this PhanKhaiKinhPhi entity, List<TepDinhKem>? files) => new() {
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        SoToTrinh = entity.SoToTrinh,
        NgayToTrinh = entity.NgayToTrinh,
        NguonVonId = entity.NguonVonId,
        KinhPhiDeXuat = entity.KinhPhiDeXuat,
        KinhPhiPhanKhai = entity.KinhPhiPhanKhai,
        ThuyetMinh = entity.ThuyetMinh,
        TrangThaiId = entity.TrangThaiId,
        TenTrangThai = entity.TrangThai != null ? entity.TrangThai.Ten : null,
        DanhSachTepDinhKem = files?.ToModels(),
    };

    public static PhanKhaiKinhPhi ToEntity(this PhanKhaiKinhPhiModel model) => new() {
        Id = model.GetId(),
        DuAnId = model.DuAnId,
        SoToTrinh = model.SoToTrinh,
        NgayToTrinh = model.NgayToTrinh,
        NguonVonId = model.NguonVonId,
        KinhPhiDeXuat = model.KinhPhiDeXuat,
        KinhPhiPhanKhai = model.KinhPhiPhanKhai,
        ThuyetMinh = model.ThuyetMinh,
    };

    public static void UpdateFrom(this PhanKhaiKinhPhi entity, PhanKhaiKinhPhiModel model) {
        entity.DuAnId = model.DuAnId;
        entity.SoToTrinh = model.SoToTrinh;
        entity.NgayToTrinh = model.NgayToTrinh;
        entity.NguonVonId = model.NguonVonId;
        entity.KinhPhiDeXuat = model.KinhPhiDeXuat;
        entity.KinhPhiPhanKhai = model.KinhPhiPhanKhai;
        entity.ThuyetMinh = model.ThuyetMinh;
    }
}