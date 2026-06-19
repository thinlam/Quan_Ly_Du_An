using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Enums;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoMoiThauDienTus;

public static class HoSoMoiThauDienTuMappingConfiguration
{

    public static HoSoMoiThauDienTuModel ToModel(
        this HoSoMoiThauDienTu entity,
        List<TepDinhKem>? files = null, List<TepDinhKem>? filesChiDinhThau = null) => new()
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            HinhThucLuaChonNhaThauId = entity.HinhThucLuaChonNhaThauId,
            GoiThauId = entity.GoiThauId,
            GiaTri = entity.GiaTri,
            ThoiGianThucHien = entity.ThoiGianThucHien,
            TrangThaiDangTai = entity.TrangThaiDangTai,
            TrangThaiId = entity.TrangThaiId,
            ChiDinhThau = entity.ChiDinhThau!= null? new ChiDinhThauModel()
            {
                Id= entity.ChiDinhThau.Id,
                So= entity.ChiDinhThau.So,
                Ngay = entity.ChiDinhThau.Ngay,
                NguoiKy = entity.ChiDinhThau.NguoiKy,
                ChucVu = entity.ChiDinhThau.ChucVu,
                TrichYeu = entity.ChiDinhThau.TrichYeu,
                DanhSachTepDinhKem = filesChiDinhThau?.Select(f => new TepDinhKemModel
                {
                    Id = f.Id,
                    ParentId = f.ParentId,
                    GroupId = f.GroupId,
                    GroupType = f.GroupType,
                    FileName = f.FileName,
                    OriginalName = f.OriginalName,
                    Path = f.Path,
                    Size = f.Size,
                    Type = f.Type,
                }).ToList()
            } : null,
            DanhSachTepDinhKem = files?.Select(f => new TepDinhKemModel
            {
                Id = f.Id,
                ParentId = f.ParentId,
                GroupId = f.GroupId,
                GroupType = f.GroupType,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size,
                Type = f.Type,
            }).ToList()
        };

    public static HoSoMoiThauDienTuInsertDto ToInsertDto(this HoSoMoiThauDienTuModel model)
    {

        HoSoMoiThauDienTuInsertDto dto = new HoSoMoiThauDienTuInsertDto()
        {
            DuAnId = model.DuAnId,
            BuocId = model.BuocId,
            HinhThucLuaChonNhaThauId = model.HinhThucLuaChonNhaThauId,
            GoiThauId = model.GoiThauId,
            GiaTri = model.GiaTri,
            ThoiGianThucHien = model.ThoiGianThucHien,
            TrangThaiDangTai = model.TrangThaiDangTai,
            TrangThaiId = model.TrangThaiId,
            ChiDinhThau = new ChiDinhThauDto()
            {
                So = model.ChiDinhThau.So,
                TrichYeu = model.ChiDinhThau.TrichYeu,
                Ngay = model.ChiDinhThau.Ngay,
                NguoiKy = model.ChiDinhThau.NguoiKy,
                ChucVu = model.ChiDinhThau.ChucVu,
                DanhSachTepDinhKem = model.ChiDinhThau?.DanhSachTepDinhKem?.Select(m => new TepDinhKemDto
                {
                    Id = m.Id,
                    ParentId = m.ParentId,
                    FileName = m.FileName,
                    OriginalName = m.OriginalName,
                    Path = m.Path,
                    Size = m.Size,
                    Type = m.Type,
                }).ToList()
            },
            DanhSachTepDinhKem = model.DanhSachTepDinhKem?.Select(m => new TepDinhKemDto
            {
                Id = m.Id,
                ParentId = m.ParentId,
                GroupId = m.GroupId,
                GroupType = m.GroupType,
                FileName = m.FileName,
                OriginalName = m.OriginalName,
                Path = m.Path,
                Size = m.Size,
                Type = m.Type,
            }).ToList()
        };
        
        return dto;


    }

    public static HoSoMoiThauDienTuUpdateModel ToUpdateModel(this HoSoMoiThauDienTuModel model) => new()
    {
        Id = model.GetId(),
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        HinhThucLuaChonNhaThauId = model.HinhThucLuaChonNhaThauId,
        GoiThauId = model.GoiThauId,
        GiaTri = model.GiaTri,
        ThoiGianThucHien = model.ThoiGianThucHien,
        TrangThaiDangTai = model.TrangThaiDangTai,
        TrangThaiId = model.TrangThaiId,
        ChiDinhThau = new ChiDinhThauDto()
        {
            So = model.ChiDinhThau.So,
            TrichYeu = model.ChiDinhThau.TrichYeu,
            Ngay = model.ChiDinhThau.Ngay,
            NguoiKy = model.ChiDinhThau.NguoiKy,
            ChucVu = model.ChiDinhThau.ChucVu,
        }
    };
    public static List<TepDinhKem> GetDanhSachTepDinhKemChiDinh(
       this ChiDinhThauModel model, long groupId)
       => model.DanhSachTepDinhKem?
           .Select(m => new TepDinhKem
           {
               Id = m.Id ?? Guid.NewGuid(),
               ParentId = m.ParentId,
               GroupId = groupId.ToString(),
               GroupType = EGroupType.HoSoMoiThauDienTuChiDinh.ToString(),
               Type = m.Type,
               FileName = m.FileName,
               OriginalName = m.OriginalName,
               Path = m.Path,
               Size = m.Size
           }).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKem(
        this HoSoMoiThauDienTuModel model, Guid groupId)
        => model.DanhSachTepDinhKem?
            .Select(m => new TepDinhKem
            {
                Id = m.Id ?? Guid.NewGuid(),
                ParentId = m.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.HoSoMoiThauDienTu.ToString(),
                Type = m.Type,
                FileName = m.FileName,
                OriginalName = m.OriginalName,
                Path = m.Path,
                Size = m.Size
            }).ToList() ?? [];
}
