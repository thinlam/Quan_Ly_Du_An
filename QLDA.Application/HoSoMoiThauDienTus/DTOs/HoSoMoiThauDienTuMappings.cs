using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public static class HoSoMoiThauDienTuMappings {
    public static HoSoMoiThauDienTu ToEntity(this HoSoMoiThauDienTuInsertDto dto)
    {
        var entity = new HoSoMoiThauDienTu()
        {
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
        if (dto.ChiDinhThau == null)
            entity.ChiDinhThau = null;
        entity.ChiDinhThau = new ChiDinhThau
        {
            Id = dto.ChiDinhThau.Id,
            NguoiKy = dto.ChiDinhThau.NguoiKy,
            Ngay = dto.ChiDinhThau.Ngay,
            So = dto.ChiDinhThau.So,
            ChucVu = dto.ChiDinhThau.ChucVu,
            TrichYeu = dto.ChiDinhThau.TrichYeu
        };

        return entity;

    }
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


   
}