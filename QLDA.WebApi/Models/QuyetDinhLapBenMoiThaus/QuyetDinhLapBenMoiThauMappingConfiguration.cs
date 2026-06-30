using QLDA.WebApi.Models.QuyetDinhLapBenMoiThaus;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.QuyetDinhLapBenMoiThaus;

public static class QuyetDinhLapBenMoiThauMappingConfiguration
{
    public static QuyetDinhLapBenMoiThauModel ToModel(this QuyetDinhLapBenMoiThau entity,
        List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new()
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId == 0 ? null : entity.BuocId,
            SoQuyetDinh = entity.So, //Số quyết định
            CoQuanQuyetDinh = entity.CoQuanQuyetDinh, 
            NgayQuyetDinh = entity.Ngay, //Ngày quyết định
            TrichYeu = entity.TrichYeu,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy,
            NoiDung = entity.NoiDung,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.QuyetDinhLapBenMoiThau))
                .Select(o => o.ToModel()).ToList(),
        };


    public static QuyetDinhLapBenMoiThau ToEntity(this QuyetDinhLapBenMoiThauModel model)
        => new()
        {
            Id = model.GetId(),
            DuAnId = model.DuAnId,
            BuocId = model.BuocId == 0 ? null : model.BuocId,
            So = model.SoQuyetDinh, //Số quyết định
            CoQuanQuyetDinh = model.CoQuanQuyetDinh,
            Ngay = model.NgayQuyetDinh, //Ngày quyết định
            TrichYeu = model.TrichYeu,
            NgayKy = model.NgayKy,
            NguoiKy = model.NguoiKy,
            NoiDung = model.NoiDung,
        };


    public static void Update(this QuyetDinhLapBenMoiThau entity, QuyetDinhLapBenMoiThauModel model)
    {
        entity.DuAnId = model.DuAnId;
        entity.BuocId = model.BuocId == 0 ? null : model.BuocId;
        entity.So = model.SoQuyetDinh; //Số quyết định
        entity.Ngay = model.NgayQuyetDinh; //Ngày quyết định
        entity.CoQuanQuyetDinh = model.CoQuanQuyetDinh;
        entity.TrichYeu = model.TrichYeu;
        entity.NgayKy = model.NgayKy;
        entity.NguoiKy = model.NguoiKy;
        entity.NoiDung = model.NoiDung;
    }
}