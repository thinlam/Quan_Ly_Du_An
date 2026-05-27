using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.ToTrinhPheDuyets;

public static class ToTrinhPheDuyetMappingConfiguration {
    public static ToTrinhPheDuyetModel ToModel(this ToTrinhPheDuyet entity,
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


    public static ToTrinhPheDuyet ToEntity(this ToTrinhPheDuyetModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TrichYeu = model.TrichYeu,
            So = model.So,
            Loai= model.Loai,
            NgayToTrinh = model.Ngay,
        };

    public static void Update(this ToTrinhPheDuyet entity, ToTrinhPheDuyetModel model) {
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.TrichYeu = model.TrichYeu;
        entity.Loai = model.Loai;
        entity.So = model.So;
        entity.NgayToTrinh = model.Ngay;
    }
}