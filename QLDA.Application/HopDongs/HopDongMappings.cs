using BuildingBlocks.CrossCutting.ExtensionMethods;
using QLDA.Application.HopDongs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.HopDongs;

public static class HopDongMappings {
    public static HopDong ToEntity(this HopDongInsertDto dto) {
        return new HopDong {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            GoiThauId = dto.GoiThauId,
            Ten = dto.Ten,
            SoHopDong = dto.SoHopDong,
            NoiDung = dto.NoiDung,
            DonViThucHienId = dto.DonViThucHienId,
            NgayKy = dto.NgayKy,
            GiaTri = dto.GiaTri,
            NgayHieuLuc = dto.NgayHieuLuc.ToStartOfDayUtc(),
            NgayDuKienKetThucHopDong = dto.NgayDuKienKetThucHopDong.ToStartOfDayUtc(),
            NgayDuKienKetThucGoiThau = dto.NgayDuKienKetThucGoiThau.ToStartOfDayUtc(),
            LoaiHopDongId = dto.LoaiHopDongId,
            IsBienBan = dto.IsBienBan
        };
    }

    public static HopDong ToEntity(this HopDongUpdateDto dto) {
        return new HopDong {
            Id = dto.Id,
            GoiThauId = dto.GoiThauId,
            Ten = dto.Ten,
            SoHopDong = dto.SoHopDong,
            NoiDung = dto.NoiDung,
            DonViThucHienId = dto.DonViThucHienId,
            NgayKy = dto.NgayKy,
            GiaTri = dto.GiaTri,
            NgayHieuLuc = dto.NgayHieuLuc.ToStartOfDayUtc(),
            NgayDuKienKetThucHopDong = dto.NgayDuKienKetThucHopDong.ToStartOfDayUtc(),
            NgayDuKienKetThucGoiThau = dto.NgayDuKienKetThucGoiThau.ToStartOfDayUtc(),
            LoaiHopDongId = dto.LoaiHopDongId,
            IsBienBan = dto.IsBienBan
        };
    }

    public static HopDongDto ToDto(this HopDong entity, IEnumerable<TepDinhKem>? files = null) {
        return new HopDongDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            GoiThauId = entity.GoiThauId,
            Ten = entity.Ten,
            SoHopDong = entity.SoHopDong,
            NoiDung = entity.NoiDung,
            DonViThucHienId = entity.DonViThucHienId,
            NgayKy = entity.NgayKy,
            GiaTri = entity.GiaTri,
            NgayHieuLuc = entity.NgayHieuLuc.ToDateOnlyVn(),
            NgayDuKienKetThucHopDong = entity.NgayDuKienKetThucHopDong.ToDateOnlyVn(),
            NgayDuKienKetThucGoiThau = entity.NgayDuKienKetThucGoiThau.ToDateOnlyVn(),
            LoaiHopDongId = entity.LoaiHopDongId,
            IsBienBan = entity.IsBienBan,
            DanhSachTepDinhKem = [.. files?.ToDtos() ?? []]
        };
    }
    public static void Update(this HopDong entity, HopDongUpdateDto model) {
        entity.GoiThauId = model.GoiThauId;
        entity.Ten = model.Ten;
        entity.SoHopDong = model.SoHopDong;
        entity.NoiDung = model.NoiDung;
        entity.DonViThucHienId = model.DonViThucHienId;
        entity.NgayKy = model.NgayKy;
        entity.GiaTri = model.GiaTri;
        entity.NgayHieuLuc = model.NgayHieuLuc.ToStartOfDayUtc();
        entity.NgayDuKienKetThucHopDong = model.NgayDuKienKetThucHopDong.ToStartOfDayUtc();
        entity.NgayDuKienKetThucGoiThau = model.NgayDuKienKetThucGoiThau.ToStartOfDayUtc();
        entity.LoaiHopDongId = model.LoaiHopDongId;
        entity.IsBienBan = model.IsBienBan;
    }
}