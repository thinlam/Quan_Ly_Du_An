using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.QuyetDinhDuyetDuAnHangMucs;

public static class QuyetDinhDuyetDuAnHangMucMappingConfiguration {
    public static QuyetDinhDuyetDuAnHangMucModel ToModel(this QuyetDinhDuyetDuAnHangMuc entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            TenHangMuc = entity.TenHangMuc,
            TongMucDauTu = entity.TongMucDauTu,
            QuyMoHangMuc = entity.QuyMoHangMuc,
        };


    public static QuyetDinhDuyetDuAnHangMuc ToEntity(this QuyetDinhDuyetDuAnHangMucModel model,
        Guid newId, Guid? existingId)
        => new() {
            Id = model.Id ?? 0,
            QuyetDinhDuyetDuAnNguonVonId = existingId ?? newId,
            TenHangMuc = model.TenHangMuc,
            TongMucDauTu = model.TongMucDauTu,
            QuyMoHangMuc = model.QuyMoHangMuc,
        };

    public static void Update(this QuyetDinhDuyetDuAnHangMuc entity, QuyetDinhDuyetDuAnHangMucModel model) {
        entity.TenHangMuc = model.TenHangMuc;
        entity.TongMucDauTu = model.TongMucDauTu;
        entity.QuyMoHangMuc = model.QuyMoHangMuc;
    }
}
