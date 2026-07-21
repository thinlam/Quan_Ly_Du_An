using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.PhuLucHopDongs;

public static class PhuLucHopDongMappingConfiguration {
    public static PhuLucHopDongModel ToModel(this PhuLucHopDong entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            Ten = entity.Ten,
            SoPhuLucHopDong = entity.SoPhuLucHopDong,
            NoiDung = entity.NoiDung,
            Ngay = entity.Ngay,
            HopDongId = entity.HopDongId,
            GiaTri = entity.GiaTri,
            NgayDuKienKetThuc = entity.NgayDuKienKetThuc,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.PhuLucHopDong))
                .Select(o => o.ToModel()).ToList()
        };


    public static PhuLucHopDong ToEntity(this PhuLucHopDongModel model)
        => new() {
            Id = model.GetId(),
            DuAnId = model.DuAnId,
            BuocId = model.BuocId,
            Ten = model.Ten,
            SoPhuLucHopDong = model.SoPhuLucHopDong,
            NoiDung = model.NoiDung,
            Ngay = model.Ngay,
            HopDongId = model.HopDongId,
            GiaTri = model.GiaTri,
            NgayDuKienKetThuc = model.NgayDuKienKetThuc,
        };

    public static void Update(this PhuLucHopDong entity, PhuLucHopDongModel model) {
        entity.DuAnId = model.DuAnId;
        entity.BuocId = model.BuocId;
        entity.Ten = model.Ten;
        entity.SoPhuLucHopDong = model.SoPhuLucHopDong;
        entity.NoiDung = model.NoiDung;
        entity.Ngay = model.Ngay;
        entity.HopDongId = model.HopDongId;
        entity.GiaTri = model.GiaTri;
        entity.NgayDuKienKetThuc = model.NgayDuKienKetThuc;
    }
}
