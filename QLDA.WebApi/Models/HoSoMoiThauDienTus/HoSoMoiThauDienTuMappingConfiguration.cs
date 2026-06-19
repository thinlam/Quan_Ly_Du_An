using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Enums;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoMoiThauDienTus;

public static class HoSoMoiThauDienTuMappingConfiguration
{

    public static HoSoMoiThauDienTuModel ToModel(
        this HoSoMoiThauDienTu entity,
        List<TepDinhKem>? files = null, List<TepDinhKem>? filesCamKet= null, List<TepDinhKem>? filesThamDinh = null, List<TepDinhKem>? fileBaoCao = null, List<TepDinhKem>? filesToTrinhQuyetDinh = null) => new()
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
            HoSoMoiThauThamDinh =  entity.NhaThauId != null ?new HoSoMoiThauThamDinhModel()
            {
                NhaThauId = entity.NhaThauId?? new Guid(),
                DinhKemQuyetDinh = filesThamDinh?.Select(f => new TepDinhKemModel
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
                }).ToList(),
                DinhKemCamKet = filesCamKet?.Select(f => new TepDinhKemModel
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
                }).ToList(),
                DinhKemBaoCao = fileBaoCao?.Select(f => new TepDinhKemModel
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
            }: null,
            ToTrinhQuyetDinh = entity.ToTrinhQuyetDinh!= null? new ToTrinhQuyetDinhModel()
            {
                Id= entity.ToTrinhQuyetDinh.Id,
                So= entity.ToTrinhQuyetDinh.So,
                Ngay = entity.ToTrinhQuyetDinh.Ngay,
                NguoiKy = entity.ToTrinhQuyetDinh.NguoiKy,
                ChucVu = entity.ToTrinhQuyetDinh.ChucVu,
                TrichYeu = entity.ToTrinhQuyetDinh.TrichYeu,
                DanhSachTepDinhKem = filesToTrinhQuyetDinh?.Select(f => new TepDinhKemModel
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
            HoSoMoiThauThamDinh = new HoSoMoiThauThamDinhDto()
            {
                NhaThauId = model.HoSoMoiThauThamDinh.GetId(),
                DinhKemCamKet = model.HoSoMoiThauThamDinh?.DinhKemCamKet?.Select(m => new TepDinhKemDto
                {
                    Id = m.Id,
                    ParentId = m.ParentId,
                    FileName = m.FileName,
                    OriginalName = m.OriginalName,
                    Path = m.Path,
                    Size = m.Size,
                    Type = m.Type,
                }).ToList(),
                DinhKemQuyetDinh = model.HoSoMoiThauThamDinh?.DinhKemQuyetDinh?.Select(m => new TepDinhKemDto
                {
                    Id = m.Id,
                    ParentId = m.ParentId,
                    FileName = m.FileName,
                    OriginalName = m.OriginalName,
                    Path = m.Path,
                    Size = m.Size,
                    Type = m.Type,
                }).ToList(),
                DinhKemBaoCao = model.HoSoMoiThauThamDinh?.DinhKemBaoCao?.Select(m => new TepDinhKemDto
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
            ToTrinhQuyetDinh = new ToTrinhQuyetDinhDto()
            {
                So = model.ToTrinhQuyetDinh.So,
                TrichYeu = model.ToTrinhQuyetDinh.TrichYeu,
                Ngay = model.ToTrinhQuyetDinh.Ngay,
                NguoiKy = model.ToTrinhQuyetDinh.NguoiKy,
                ChucVu = model.ToTrinhQuyetDinh.ChucVu,
                DanhSachTepDinhKem = model.ToTrinhQuyetDinh?.DanhSachTepDinhKem?.Select(m => new TepDinhKemDto
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
        ToTrinhQuyetDinh = new ToTrinhQuyetDinhDto()
        {
            So = model.ToTrinhQuyetDinh.So,
            TrichYeu = model.ToTrinhQuyetDinh.TrichYeu,
            Ngay = model.ToTrinhQuyetDinh.Ngay,
            NguoiKy = model.ToTrinhQuyetDinh.NguoiKy,
            ChucVu = model.ToTrinhQuyetDinh.ChucVu,
        }
    };
    public static List<TepDinhKem> GetDanhSachTepDinhKemBaoCaoThamDinh(
       this HoSoMoiThauThamDinhModel model, Guid groupId)
       => model.DinhKemBaoCao?
           .Select(m => new TepDinhKem
           {
               Id = m.Id ?? Guid.NewGuid(),
               ParentId = m.ParentId,
               GroupId = groupId.ToString(),
               GroupType = EGroupType.HoSoMoiThauDienTuBaoCaoTD.ToString(),
               Type = m.Type,
               FileName = m.FileName,
               OriginalName = m.OriginalName,
               Path = m.Path,
               Size = m.Size
           }).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKemCamKetThamDinh(
       this HoSoMoiThauThamDinhModel model, Guid groupId)
       => model.DinhKemCamKet?
           .Select(m => new TepDinhKem
           {
               Id = m.Id ?? Guid.NewGuid(),
               ParentId = m.ParentId,
               GroupId = groupId.ToString(),
               GroupType = EGroupType.HoSoMoiThauDienTuCamKetTD.ToString(),
               Type = m.Type,
               FileName = m.FileName,
               OriginalName = m.OriginalName,
               Path = m.Path,
               Size = m.Size
           }).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKemQuyetDinhThamDinh(
       this HoSoMoiThauThamDinhModel model, Guid groupId)
       => model.DinhKemQuyetDinh?
           .Select(m => new TepDinhKem
           {
               Id = m.Id ?? Guid.NewGuid(),
               ParentId = m.ParentId,
               GroupId = groupId.ToString(),
               GroupType = EGroupType.HoSoMoiThauDienTuQuyetDinhTD.ToString(),
               Type = m.Type,
               FileName = m.FileName,
               OriginalName = m.OriginalName,
               Path = m.Path,
               Size = m.Size
           }).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKemToTrinh(
       this ToTrinhQuyetDinhModel model, long groupId)
       => model.DanhSachTepDinhKem?
           .Select(m => new TepDinhKem
           {
               Id = m.Id ?? Guid.NewGuid(),
               ParentId = m.ParentId,
               GroupId = groupId.ToString(),
               GroupType = EGroupType.HoSoMoiThauDienTuToTrinh.ToString(),
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
