using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.VanBanChuTruongs;

public static class VanBanChuTruongMappingConfiguration {
    public static VanBanChuTruongModel ToModel(this VanBanChuTruong entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            ChucVuId = entity.ChucVuId,
            LoaiVanBanId = entity.LoaiVanBanId,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy,
            TrichYeu = entity.TrichYeu,
            SoVanBan = entity.So,
            NgayVanBan = entity.Ngay,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.VanBanChuTruong))
                .Select(o => o.ToModel()).ToList()
        };


    public static VanBanChuTruong ToEntity(this VanBanChuTruongModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            ChucVuId = model.ChucVuId,
            LoaiVanBanId = model.LoaiVanBanId,
            NgayKy = model.NgayKy,
            NguoiKy = model.NguoiKy,
            TrichYeu = model.TrichYeu,
            So = model.SoVanBan,
            Ngay = model.NgayVanBan,
        };

    public static void Update(this VanBanChuTruong entity, VanBanChuTruongModel model) {
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.ChucVuId = model.ChucVuId;
        entity.LoaiVanBanId = model.LoaiVanBanId;
        entity.NgayKy = model.NgayKy;
        entity.NguoiKy = model.NguoiKy;
        entity.TrichYeu = model.TrichYeu;
        entity.So = model.SoVanBan;
        entity.Ngay = model.NgayVanBan;
    }
}
