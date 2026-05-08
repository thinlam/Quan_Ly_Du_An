using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public static class HoSoMoiThauDienTuMappings {
    public static HoSoMoiThauDienTu ToEntity(this HoSoMoiThauDienTuInsertDto dto) => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        DuAnId = dto.DuAnId,
        BuocId = dto.BuocId,
        HinhThucLuaChonNhaThauId = dto.HinhThucLuaChonNhaThauId,
        GoiThauId = dto.GoiThauId,
        GiaTri = dto.GiaTri,
        ThoiGianThucHien = dto.ThoiGianThucHien,
        TrangThaiDangTai = dto.TrangThaiDangTai,
        TrangThaiId = dto.TrangThaiId,
    };

    public static void Update(this HoSoMoiThauDienTu entity, HoSoMoiThauDienTuUpdateModel dto) {
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        entity.HinhThucLuaChonNhaThauId = dto.HinhThucLuaChonNhaThauId;
        entity.GoiThauId = dto.GoiThauId;
        entity.GiaTri = dto.GiaTri;
        entity.ThoiGianThucHien = dto.ThoiGianThucHien;
        entity.TrangThaiDangTai = dto.TrangThaiDangTai;
        entity.TrangThaiId = dto.TrangThaiId;
    }

    public static HoSoMoiThauDienTuDto ToDto(this HoSoMoiThauDienTu entity) =>
        entity.ToDto(null);

    public static HoSoMoiThauDienTuDto ToDto(this HoSoMoiThauDienTu entity,
        List<TepDinhKem>? files) => new() {
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        TenDuAn = entity.DuAn?.TenDuAn,
        BuocId = entity.BuocId,
        TenBuoc = entity.Buoc?.TenBuoc,
        HinhThucLuaChonNhaThauId = entity.HinhThucLuaChonNhaThauId,
        TenHinhThucLuaChonNhaThau = entity.HinhThucLuaChonNhaThau?.Ten,
        GoiThauId = entity.GoiThauId,
        TenGoiThau = entity.GoiThau?.Ten,
        GiaTri = entity.GiaTri,
        ThoiGianThucHien = entity.ThoiGianThucHien,
        TrangThaiDangTai = entity.TrangThaiDangTai,
        TrangThaiId = entity.TrangThaiId,
        TenTrangThai = entity.TrangThaiPheDuyet?.Ten,
        DanhSachTepDinhKem = files?.Select(f => new TepDinhKemDto {
            Id = f.Id,
            ParentId = f.ParentId,
            GroupId = f.GroupId,
            GroupType = f.GroupType,
            FileName = f.FileName,
            OriginalName = f.OriginalName,
            Path = f.Path,
            Size = f.Size,
            Type = f.Type,
        }).ToList(),
    };
}