using QLDA.WebApi.Models.DonViTuVanKeHoachs;
using QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.TrienKhaiKeHoachLCNTs;

public static class TrienKhaiKeHoachLCNTMappingConfiguration {
    public static TrienKhaiKeHoachLCNTModel ToModel(this TrienKhaiKeHoachLCNT entity, List<DonViTuVanKeHoachModel>? donViTuVan = null,
        List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            NgayTrinh = entity.NgayTrinh,
            HinhThucLCNT = entity.HinhThucLCNT,
            GoiThauId = entity.GoiThauId,
            TrangThaiId = entity.TrangThaiId,
            TrangThaiDangTaiId = entity.TrangThaiDangTaiId,
            GiaTri = entity.GiaTri,
            ThoiGianThucHien = entity.ThoiGianThucHien,
            NoiDung = entity.NoiDung,
            YeuCau = entity.YeuCau,
            DonViTuVans = donViTuVan,
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList()
        };


    public static TrienKhaiKeHoachLCNT ToEntity(this TrienKhaiKeHoachLCNTModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TrichYeu = model.TrichYeu,
            So = model.So,
            NgayTrinh = model.NgayTrinh,
            HinhThucLCNT = model.HinhThucLCNT,
            GoiThauId = model.GoiThauId,
            TrangThaiId = model.TrangThaiId,
            TrangThaiDangTaiId = model.TrangThaiDangTaiId,
            GiaTri = model.GiaTri,
            ThoiGianThucHien = model.ThoiGianThucHien,
            NoiDung = model.NoiDung,
            YeuCau = model.YeuCau,
            DonViTuVans = model.DonViTuVans?.Select(x => x.ToEntity(model.GetId())).ToList() ?? [],

        };

  
}