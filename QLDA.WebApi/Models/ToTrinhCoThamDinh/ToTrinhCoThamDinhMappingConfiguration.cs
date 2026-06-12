using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.ToTrinhCoThamDinhModels;

namespace QLDA.WebApi.Models.ToTrinhCoThamDinhs;

public static class ToTrinhCoThamDinhMappingConfiguration {
    public static ToTrinhCoThamDinhModel ToModel(this ToTrinhCoThamDinh entity,
        List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            Ngay = entity.NgayToTrinh,
            Loai= entity.Loai,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                .Select(o => o.ToModel()).ToList()
        };


    public static ToTrinhCoThamDinh ToEntity(this ToTrinhCoThamDinhModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TrichYeu = model.TrichYeu,
            So = model.So,
            Loai= model.Loai,
            NgayToTrinh = model.Ngay,
        };

    public static void Update(this ToTrinhCoThamDinh entity, ToTrinhCoThamDinhModel model) {
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.TrichYeu = model.TrichYeu;
        entity.Loai = model.Loai;
        entity.So = model.So;
        entity.NgayToTrinh = model.Ngay;
    }
}