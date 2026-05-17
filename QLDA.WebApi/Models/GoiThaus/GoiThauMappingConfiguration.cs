using QLDA.Application.GoiThaus.DTOs;

namespace QLDA.WebApi.Models.GoiThaus;

public static class GoiThauMappingConfiguration {
    public static GoiThauImportDto ToImportDto(this GoiThauImportModel model)
        => new() {
            Stt = model.Stt,
            TenDuAn = model.TenDuAn,
            TenKeHoachLuaChonNhaThau = model.TenKeHoachLuaChonNhaThau,
            TenBuoc = model.TenBuoc,
            Ten = model.Ten,
            TomTatCongViecChinhGoiThau = model.TomTatCongViecChinhGoiThau,
            GiaTri = model.GiaTri,
            TenNguonVon = model.TenNguonVon,
            TenHinhThucLuaChonNhaThau = model.TenHinhThucLuaChonNhaThau,
            TenPhuongThucLuaChonNhaThau = model.TenPhuongThucLuaChonNhaThau,
            TenLoaiHopDong = model.TenLoaiHopDong,
            ThoiGianToChucLuaChonNhaThau = model.ThoiGianToChucLuaChonNhaThau,
            ThoiGianBatDauToChucLuaChonNhaThau = model.ThoiGianBatDauToChucLuaChonNhaThau,
            ThoiGianThucHienGoiThau = model.ThoiGianThucHienGoiThau,
            TuyChonMuaThem = model.TuyChonMuaThem,
            GiamSatHoatDongDauThau = model.GiamSatHoatDongDauThau
        };

    public static List<GoiThauImportDto> ToImportDtoList(this List<GoiThauImportModel> entities)
        => [.. entities.Select(e => e.ToImportDto())];
}