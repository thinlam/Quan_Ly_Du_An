using BuildingBlocks.Application.Attachments.Common;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.KhoKhanVuongMacs;

public static class KhoKhanVuongMacMappingConfiguration {
    public static KhoKhanVuongMacModel ToModel(this BaoCaoKhoKhanVuongMac entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            Ngay = entity.Ngay,
            NoiDung = entity.NoiDung,
            TinhTrangId = entity.TinhTrangId,
            MucDoKhoKhanId = entity.MucDoKhoKhanId,
            HuongXuLy = entity.HuongXuLy,
            KetQua = new KetQuaXuLyModel() {
                KetQuaXuLy = entity.KetQuaXuLy,
                NgayXuLy = entity.NgayXuLy,
                DanhSachTepDinhKem = danhSachTepDinhKem?
                    .Where(o => SignedGroupTypeHelper.Prefix + nameof(EGroupType.KetQuaXuLyKhoKhanVuongMac) == o.GroupType || o.GroupType == nameof(EGroupType.KetQuaXuLyKhoKhanVuongMac))
                    .Select(o => o.ToModel()).ToList()
            },
            DanhSachTepDinhKem = danhSachTepDinhKem?
                .Where(o => SignedGroupTypeHelper.Prefix + nameof(EGroupType.KhoKhanVuongMac) == o.GroupType || o.GroupType == nameof(EGroupType.KhoKhanVuongMac))
                .Select(o => o.ToModel()).ToList()
        };


    public static BaoCaoKhoKhanVuongMac ToEntity(this KhoKhanVuongMacModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            Ngay = model.Ngay,
            NoiDung = model.NoiDung,
            TinhTrangId = model.TinhTrangId,
            MucDoKhoKhanId = model.MucDoKhoKhanId,
            HuongXuLy = model.HuongXuLy,
            KetQuaXuLy = model.KetQua?.KetQuaXuLy,
            NgayXuLy = model.KetQua?.NgayXuLy,
        };

    public static void Update(this BaoCaoKhoKhanVuongMac entity, KhoKhanVuongMacModel model) {
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.Ngay = model.Ngay;
        entity.NoiDung = model.NoiDung;
        entity.HuongXuLy = model.HuongXuLy;
        entity.TinhTrangId = model.TinhTrangId;
        entity.MucDoKhoKhanId = model.MucDoKhoKhanId;
        entity.KetQuaXuLy = model.KetQua?.KetQuaXuLy;
        entity.NgayXuLy = model.KetQua?.NgayXuLy;
    }
}
