using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public static class HoSoMoiThauDienTuMappings
{
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
            NhaThauId = dto.HoSoMoiThauThamDinh.NhaThauId,

        };
        if (dto.ToTrinhQuyetDinh == null)
            entity.ToTrinhQuyetDinh = null;
        else
        {

            entity.ToTrinhQuyetDinh = new ToTrinhQuyetDinh
            {
                Id = dto.ToTrinhQuyetDinh.Id,
                NguoiKy = dto.ToTrinhQuyetDinh.NguoiKy,
                Ngay = dto.ToTrinhQuyetDinh.Ngay,
                So = dto.ToTrinhQuyetDinh.So,
                ChucVu = dto.ToTrinhQuyetDinh.ChucVu,
                TrichYeu = dto.ToTrinhQuyetDinh.TrichYeu
            };
        }

        return entity;

    }
    public static void Update(this HoSoMoiThauDienTu entity, HoSoMoiThauDienTuUpdateModel dto)
    {
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