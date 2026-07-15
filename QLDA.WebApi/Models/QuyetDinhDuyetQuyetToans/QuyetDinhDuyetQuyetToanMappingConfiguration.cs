using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.QuyetDinhDuyetQuyetToans;

public static class QuyetDinhDuyetQuyetToanMappingConfiguration {
    public static QuyetDinhDuyetQuyetToanModel ToModel(this QuyetDinhDuyetQuyetToan entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            SoQuyetDinh = entity.So,
            NgayQuyetDinh = entity.Ngay,
            CoQuanQuyetDinh = entity.CoQuanQuyetDinh,
            TrichYeu = entity.TrichYeu,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy,
            GiaTri = entity.GiaTri,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.QuyetDinhDuyetQuyetToan))
                .Select(o => o.ToModel()).ToList()
        };


    public static QuyetDinhDuyetQuyetToan ToEntity(this QuyetDinhDuyetQuyetToanModel model)
        => new() {
            Id = model.GetId(),
            DuAnId = model.DuAnId,
            BuocId = model.BuocId,
            So = model.SoQuyetDinh,
            Ngay = model.NgayQuyetDinh,
            CoQuanQuyetDinh = model.CoQuanQuyetDinh,
            TrichYeu = model.TrichYeu,
            NgayKy = model.NgayKy,
            NguoiKy = model.NguoiKy,
            GiaTri = model.GiaTri,
        };

    public static void Update(this QuyetDinhDuyetQuyetToan entity, QuyetDinhDuyetQuyetToanModel model) {
        entity.DuAnId = model.DuAnId;
        entity.BuocId = model.BuocId;
        entity.So = model.SoQuyetDinh;
        entity.Ngay = model.NgayQuyetDinh;
        entity.CoQuanQuyetDinh = model.CoQuanQuyetDinh;
        entity.TrichYeu = model.TrichYeu;
        entity.NgayKy = model.NgayKy;
        entity.NguoiKy = model.NguoiKy;
        entity.GiaTri = model.GiaTri;
    }
}
