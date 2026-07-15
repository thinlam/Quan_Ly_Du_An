using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.QuyetDinhDuyetDuAnNguonVons;

public static class QuyetDinhDuyetDuAnNguonVonMappingConfiguration {
    public static QuyetDinhDuyetDuAnNguonVonModel ToModel(this QuyetDinhDuyetDuAnNguonVon entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            NguonVonId = entity.NguonVonId,
            GiaTri = entity.GiaTri,
        };


    public static QuyetDinhDuyetDuAnNguonVon ToEntity(this QuyetDinhDuyetDuAnNguonVonModel model,
        Guid quyetDinhDuyetDuAnId)
        => new() {
            Id = model.GetId(),
            QuyetDinhDuyetDuAnId = quyetDinhDuyetDuAnId,
            NguonVonId = model.NguonVonId,
            GiaTri = model.GiaTri,
        };

    public static void Update(this QuyetDinhDuyetDuAnNguonVon entity, QuyetDinhDuyetDuAnNguonVonModel model) {
        entity.NguonVonId = model.NguonVonId;
        entity.GiaTri = model.GiaTri;
    }
}
