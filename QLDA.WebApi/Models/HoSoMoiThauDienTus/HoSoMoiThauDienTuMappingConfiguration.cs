using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Enums;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoMoiThauDienTus;

public static class HoSoMoiThauDienTuMappingConfiguration
{

    public static HoSoMoiThauDienTuModel ToModel(
        this HoSoMoiThauDienTu entity,
        List<TepDinhKem>? files = null, List<TepDinhKem>? filesCamKet= null, List<TepDinhKem>? filesThamDinh = null
        , List<TepDinhKem>? fileBaoCao = null, List<TepDinhKem>? filesToTrinh = null
        , List<TepDinhKem>? filesQuyetDinh = null) => new()
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
            ToTrinh = entity.ToTrinh!= null? new ToTrinhQuyetDinhModel()
            {
                Id= entity.ToTrinh.Id,
                So= entity.ToTrinh.So,
                Ngay = entity.ToTrinh.Ngay,
                NguoiKy = entity.ToTrinh.NguoiKy,
                ChucVu = entity.ToTrinh.ChucVu,
                TrichYeu = entity.ToTrinh.TrichYeu,
                DanhSachTepDinhKem = filesToTrinh?.Select(f => new TepDinhKemModel
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
            QuyetDinh = entity.QuyetDinh != null ? new ToTrinhQuyetDinhModel()
            {
                Id = entity.QuyetDinh.Id,
                So = entity.QuyetDinh.So,
                Ngay = entity.QuyetDinh.Ngay,
                NguoiKy = entity.QuyetDinh.NguoiKy,
                ChucVu = entity.QuyetDinh.ChucVu,
                TrichYeu = entity.QuyetDinh.TrichYeu,
                DanhSachTepDinhKem = filesQuyetDinh?.Select(f => new TepDinhKemModel
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
            ToTrinh = new ToTrinhQuyetDinhDto()
            {
                So = model.ToTrinh.So,
                TrichYeu = model.ToTrinh.TrichYeu,
                Ngay = model.ToTrinh.Ngay,
                NguoiKy = model.ToTrinh.NguoiKy,
                ChucVu = model.ToTrinh.ChucVu,
                DanhSachTepDinhKem = model.ToTrinh?.DanhSachTepDinhKem?.Select(m => new TepDinhKemDto
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
           QuyetDinh  = new ToTrinhQuyetDinhDto()
             {
                 So = model.QuyetDinh.So,
                 TrichYeu = model.QuyetDinh.TrichYeu,
                 Ngay = model.QuyetDinh.Ngay,
                 NguoiKy = model.QuyetDinh.NguoiKy,
                 ChucVu = model.QuyetDinh.ChucVu,
                 DanhSachTepDinhKem = model.QuyetDinh?.DanhSachTepDinhKem?.Select(m => new TepDinhKemDto
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
        ToTrinh = new ToTrinhQuyetDinhDto()
        {
            So = model.ToTrinh.So,
            TrichYeu = model.ToTrinh.TrichYeu,
            Ngay = model.ToTrinh.Ngay,
            NguoiKy = model.ToTrinh.NguoiKy,
            ChucVu = model.ToTrinh.ChucVu,
        },
        QuyetDinh = new ToTrinhQuyetDinhDto()
        {
            So = model.QuyetDinh.So,
            TrichYeu = model.QuyetDinh.TrichYeu,
            Ngay = model.QuyetDinh.Ngay,
            NguoiKy = model.QuyetDinh.NguoiKy,
            ChucVu = model.QuyetDinh.ChucVu,
        }
    };
    public static List<TepDinhKem> GetDanhSachTepDinhKemBaoCaoThamDinh(
       this HoSoMoiThauThamDinhModel model, Guid groupId)
       => model.DinhKemBaoCao?.ToEntities(groupId, EGroupType.HoSoMoiThauDienTuBaoCaoTD).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKemCamKetThamDinh(
       this HoSoMoiThauThamDinhModel model, Guid groupId)
       => model.DinhKemCamKet?.ToEntities(groupId, EGroupType.HoSoMoiThauDienTuCamKetTD).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKemQuyetDinhThamDinh(
       this HoSoMoiThauThamDinhModel model, Guid groupId)
       => model.DinhKemQuyetDinh?.ToEntities(groupId, EGroupType.HoSoMoiThauDienTuQuyetDinhTD).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKemToTrinh(
       this ToTrinhQuyetDinhModel model, long groupId)
       => model.DanhSachTepDinhKem?.ToEntities(groupId.ToString(), EGroupType.HoSoMoiThauDienTuToTrinh).ToList() ?? [];
    public static List<TepDinhKem> GetDanhSachTepDinhKemQuyetDinh(
      this ToTrinhQuyetDinhModel model, long groupId)
      => model.DanhSachTepDinhKem?.ToEntities(groupId.ToString(), EGroupType.HoSoMoiThauDienTuToTrinh).ToList() ?? [];

    public static List<TepDinhKem> GetDanhSachTepDinhKem(
        this HoSoMoiThauDienTuModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.HoSoMoiThauDienTu).ToList() ?? [];
}
