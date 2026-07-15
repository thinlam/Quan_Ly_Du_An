using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.DeXuatNhuCauKinhPhis;

public static class DeXuatNhuCauKinhPhiMappingConfiguration
{
    public static DeXuatNhuCauKinhPhiModel ToModel(this DeXuatNhuCauKinhPhi entity, List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            DonViDeXuatId = entity.DonViDeXuatId,
            SoPhieuChuyen = entity.SoPhieuChuyen,
            NgayPhieuChuyen = entity.NgayPhieuChuyen,
            KinhPhiDeXuat = entity.KinhPhiDeXuat,
            TrichYeu = entity.TrichYeu,
            TrangThaiId = entity.TrangThaiId,

            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList()
        };


    public static DeXuatNhuCauKinhPhi ToEntity(this DeXuatNhuCauKinhPhiModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            DonViDeXuatId = model.DonViDeXuatId,
            SoPhieuChuyen = model.SoPhieuChuyen,
            NgayPhieuChuyen = model.NgayPhieuChuyen,
            KinhPhiDeXuat = model.KinhPhiDeXuat,
            TrichYeu = model.TrichYeu,
        };

    public static void Update(this DeXuatNhuCauKinhPhi entity, DeXuatNhuCauKinhPhiModel model) {
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;
    }
}
