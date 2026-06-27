using QLDA.Application.GoiThaus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.GoiThaus;

public static class GoiThauMappings {
    public static GoiThau ToEntity(this GoiThauInsertDto dto) {
        return new GoiThau {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            KeHoachLuaChonNhaThauId = dto.KeHoachLuaChonNhaThauId,
            Ten = dto.Ten,
            GiaTri = dto.GiaTri,
            NguonVonId = dto.NguonVonId,
            LoaiCongViecId = dto.LoaiCongViecId,
            HinhThucLuaChonNhaThauId = dto.HinhThucLuaChonNhaThauId,
            PhuongThucLuaChonNhaThauId = dto.PhuongThucLuaChonNhaThauId,
            ThoiGianLuaNhaThau = dto.ThoiGianLuaNhaThau,
            LoaiHopDongId = dto.LoaiHopDongId,
            ThoiGianHopDong = dto.ThoiGianHopDong,
            TomTatCongViecChinhGoiThau = dto.TomTatCongViecChinhGoiThau,
            ThoiGianBatDauToChucLuaChonNhaThau = dto.ThoiGianBatDauToChucLuaChonNhaThau,
            ThoiGianThucHienGoiThau = dto.ThoiGianThucHienGoiThau,
            TuyChonMuaThem = dto.TuyChonMuaThem,
            GiamSatHoatDongDauThau = dto.GiamSatHoatDongDauThau,
            DaDuyet = true
        };
    }

    public static GoiThau ToEntity(this GoiThauUpdateDto dto) {
        return new GoiThau {
            Id = dto.Id,
            KeHoachLuaChonNhaThauId = dto.KeHoachLuaChonNhaThauId,
            Ten = dto.Ten,
            GiaTri = dto.GiaTri,
            NguonVonId = dto.NguonVonId,
            HinhThucLuaChonNhaThauId = dto.HinhThucLuaChonNhaThauId,
            PhuongThucLuaChonNhaThauId = dto.PhuongThucLuaChonNhaThauId,
            ThoiGianLuaNhaThau = dto.ThoiGianLuaNhaThau,
            LoaiCongViecId = dto.LoaiCongViecId,
            LoaiHopDongId = dto.LoaiHopDongId,
            ThoiGianHopDong = dto.ThoiGianHopDong,
            TomTatCongViecChinhGoiThau = dto.TomTatCongViecChinhGoiThau,
            ThoiGianBatDauToChucLuaChonNhaThau = dto.ThoiGianBatDauToChucLuaChonNhaThau,
            ThoiGianThucHienGoiThau = dto.ThoiGianThucHienGoiThau,
            TuyChonMuaThem = dto.TuyChonMuaThem,
            GiamSatHoatDongDauThau = dto.GiamSatHoatDongDauThau
        };
    }

    public static GoiThauDto ToDto(this GoiThau entity, IEnumerable<TepDinhKem>? files = null) {
        return new GoiThauDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            KeHoachLuaChonNhaThauId = entity.KeHoachLuaChonNhaThauId,
            Ten = entity.Ten,
            GiaTri = entity.GiaTri,
            NguonVonId = entity.NguonVonId,
            LoaiCongViecId = entity.LoaiCongViecId,
            HinhThucLuaChonNhaThauId = entity.HinhThucLuaChonNhaThauId,
            PhuongThucLuaChonNhaThauId = entity.PhuongThucLuaChonNhaThauId,
            ThoiGianLuaNhaThau = entity.ThoiGianLuaNhaThau,
            LoaiHopDongId = entity.LoaiHopDongId,
            ThoiGianHopDong = entity.ThoiGianHopDong,
            TomTatCongViecChinhGoiThau = entity.TomTatCongViecChinhGoiThau,
            ThoiGianBatDauToChucLuaChonNhaThau = entity.ThoiGianBatDauToChucLuaChonNhaThau,
            ThoiGianThucHienGoiThau = entity.ThoiGianThucHienGoiThau,
            TuyChonMuaThem = entity.TuyChonMuaThem,
            GiamSatHoatDongDauThau = entity.GiamSatHoatDongDauThau,
            HopDongId = entity.HopDong?.Id,
            KetQuaTrungThauId = entity.KetQuaTrungThau?.Id,
            DanhSachTepDinhKem = [.. files?.ToDtos() ?? []]
        };
    }

    public static void Update(this GoiThau entity, GoiThauUpdateDto dto) {
        entity.KeHoachLuaChonNhaThauId = dto.KeHoachLuaChonNhaThauId;
        entity.Ten = dto.Ten;
        entity.GiaTri = dto.GiaTri;
        entity.NguonVonId = dto.NguonVonId;
        entity.HinhThucLuaChonNhaThauId = dto.HinhThucLuaChonNhaThauId;
        entity.PhuongThucLuaChonNhaThauId = dto.PhuongThucLuaChonNhaThauId;
        entity.ThoiGianLuaNhaThau = dto.ThoiGianLuaNhaThau;
        entity.LoaiCongViecId = dto.LoaiCongViecId;
        entity.LoaiHopDongId = dto.LoaiHopDongId;
        entity.ThoiGianHopDong = dto.ThoiGianHopDong;
        entity.TomTatCongViecChinhGoiThau = dto.TomTatCongViecChinhGoiThau;
        entity.ThoiGianBatDauToChucLuaChonNhaThau = dto.ThoiGianBatDauToChucLuaChonNhaThau;
        entity.ThoiGianThucHienGoiThau = dto.ThoiGianThucHienGoiThau;
        entity.TuyChonMuaThem = dto.TuyChonMuaThem;
        entity.GiamSatHoatDongDauThau = dto.GiamSatHoatDongDauThau;
    }
}