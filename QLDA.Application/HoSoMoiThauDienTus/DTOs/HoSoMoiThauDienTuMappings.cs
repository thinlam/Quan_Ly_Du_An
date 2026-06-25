using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public static class HoSoMoiThauDienTuMappings
{
    public static HoSoMoiThauDienTu ToEntity(this HoSoMoiThauDienTuInsertDto dto)
    {
        var id = GuidExtensions.GetSequentialGuidId();

        var entity = new HoSoMoiThauDienTu()
        {
            Id = id,
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
        if (dto.ToTrinh == null || string.IsNullOrEmpty(dto.ToTrinh.So))
            entity.ToTrinh = null;
        else
        {

            entity.ToTrinh = new ToTrinhQuyetDinh
            {
                Id = dto.ToTrinh.Id,
                NguoiKy = dto.ToTrinh.NguoiKy,
                Ngay = dto.ToTrinh.Ngay,
                So = dto.ToTrinh.So,
                ChucVu = dto.ToTrinh.ChucVu,
                TrichYeu = dto.ToTrinh.TrichYeu
            };
        }
        if (dto.QuyetDinh == null || string.IsNullOrEmpty(dto.QuyetDinh.So))
            entity.QuyetDinh = null;
        else
        {

            entity.QuyetDinh = new ToTrinhQuyetDinh
            {
                Id = dto.QuyetDinh.Id,
                NguoiKy = dto.QuyetDinh.NguoiKy,
                Ngay = dto.QuyetDinh.Ngay,
                So = dto.QuyetDinh.So,
                ChucVu = dto.QuyetDinh.ChucVu,
                TrichYeu = dto.QuyetDinh.TrichYeu
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