using QLDA.WebApi.Models.QuyetDinhLapHoiDongThamDinhs;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.QuyetDinhLapHoiDongThamDinhs;

public static class QuyetDinhLapHoiDongThamDinhMappingConfiguration {
    public static QuyetDinhLapHoiDongThamDinhModel ToModel(this QuyetDinhLapHoiDongThamDinh entity,
        List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId == 0 ? null : entity.BuocId,
            SoQuyetDinh = entity.So, //Số quyết định
            NgayQuyetDinh = entity.Ngay, //Ngày quyết định
            TrichYeu = entity.TrichYeu,
            CoQuanQuyetDinh = entity.CoQuanQuyetDinh,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy,
            NoiDung = entity.NoiDung,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.QuyetDinhLapHoiDongThamDinh))
                .Select(o => o.ToModel()).ToList(),
        };


    public static QuyetDinhLapHoiDongThamDinh ToEntity(this QuyetDinhLapHoiDongThamDinhModel model)
        => new() {
            Id = model.GetId(),
            DuAnId = model.DuAnId,
            BuocId = model.BuocId == 0 ? null : model.BuocId,
            So = model.SoQuyetDinh, //Số quyết định
            Ngay = model.NgayQuyetDinh, //Ngày quyết định
            TrichYeu = model.TrichYeu,
            CoQuanQuyetDinh = model.CoQuanQuyetDinh,
            NgayKy = model.NgayKy,
            NguoiKy = model.NguoiKy,
            NoiDung = model.NoiDung,
        };


    public static void Update(this QuyetDinhLapHoiDongThamDinh entity, QuyetDinhLapHoiDongThamDinhModel model) {
        entity.DuAnId = model.DuAnId;
        entity.BuocId = model.BuocId == 0 ? null : model.BuocId;
        entity.So = model.SoQuyetDinh; //Số quyết định
        entity.Ngay = model.NgayQuyetDinh; //Ngày quyết định
        entity.TrichYeu = model.TrichYeu;
        entity.CoQuanQuyetDinh = model.CoQuanQuyetDinh;
        entity.NgayKy = model.NgayKy;
        entity.NguoiKy = model.NguoiKy;
        entity.NoiDung = model.NoiDung;
    }
}