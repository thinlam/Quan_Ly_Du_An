using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoMoiThauDienTus;

public static class HoSoMoiThauDienTuMappingConfiguration
{

    public static HoSoMoiThauDienTuModel ToModel(
        this HoSoMoiThauDienTu entity,
        List<Attachment>? files = null, List<Attachment>? filesCamKet = null, List<Attachment>? filesThamDinh = null
        , List<Attachment>? fileBaoCao = null, List<Attachment>? filesToTrinh = null
        , List<Attachment>? filesQuyetDinh = null) => new()
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            ThamDinh = entity.ThamDinh,
            HinhThucLuaChonNhaThauId = entity.HinhThucLuaChonNhaThauId,
            GoiThauId = entity.GoiThauId,
            GiaTri = entity.GiaTri,
            ThoiGianThucHien = entity.ThoiGianThucHien,
            TrangThaiDangTai = entity.TrangThaiDangTai,
            TrangThaiId = entity.TrangThaiId,
            HoSoMoiThauThamDinh = entity.NhaThauId != null && (entity.ThamDinh??false) ? new HoSoMoiThauThamDinhModel()
            {
                NhaThauId = entity.NhaThauId ?? new Guid(),
                DinhKemQuyetDinh = filesThamDinh?.Select(f => f.ToModel()).ToList() ,
                DinhKemCamKet = filesCamKet?.Select(f => f.ToModel()).ToList(),
                DinhKemBaoCao = fileBaoCao?.Select(f => f.ToModel()).ToList()
            } : null,
            ToTrinh = entity.ToTrinh != null ? new ToTrinhQuyetDinhModel()
            {
                Id = entity.ToTrinh!.Id,
                So = entity.ToTrinh!.So,
                Ngay = entity.ToTrinh!.Ngay,
                NguoiKy = entity.ToTrinh!.NguoiKy,
                ChucVu = entity.ToTrinh!.ChucVu,
                TrichYeu = entity.ToTrinh!.TrichYeu,
                DanhSachTepDinhKem = filesToTrinh?.Select(f => f.ToModel()).ToList()
            } : null,
            QuyetDinh = entity.QuyetDinh != null ? new ToTrinhQuyetDinhModel()
            {
                Id = entity.QuyetDinh!.Id,
                So = entity.QuyetDinh!.So,
                Ngay = entity.QuyetDinh!.Ngay,
                NguoiKy = entity.QuyetDinh!.NguoiKy,
                ChucVu = entity.QuyetDinh!.ChucVu,
                TrichYeu = entity.QuyetDinh!.TrichYeu,
                DanhSachTepDinhKem = filesQuyetDinh?.Select(f => f.ToModel()).ToList()
            } : null,
            DanhSachTepDinhKem = files?.Select(f => f.ToModel()).ToList()
        };

    public static HoSoMoiThauDienTuInsertDto ToInsertDto(this HoSoMoiThauDienTuModel model)
    {

        HoSoMoiThauDienTuInsertDto dto = new HoSoMoiThauDienTuInsertDto()
        {
            DuAnId = model.DuAnId,
            BuocId = model.BuocId,
            ThamDinh = model.ThamDinh,
            HinhThucLuaChonNhaThauId = model.HinhThucLuaChonNhaThauId,
            GoiThauId = model.GoiThauId,
            GiaTri = model.GiaTri,
            ThoiGianThucHien = model.ThoiGianThucHien,
            TrangThaiDangTai = model.TrangThaiDangTai,
            TrangThaiId = model.TrangThaiId,
            HoSoMoiThauThamDinh = (model.ThamDinh ?? false) && model.HoSoMoiThauThamDinh != null ? new HoSoMoiThauThamDinhDto()
            {
                NhaThauId = model.HoSoMoiThauThamDinh!.GetId(),
                DinhKemCamKet = model.HoSoMoiThauThamDinh?.DinhKemCamKet?.Select(m => ToDto(m)).ToList(),
                DinhKemQuyetDinh = model.HoSoMoiThauThamDinh?.DinhKemQuyetDinh?.Select(m => ToDto(m)).ToList(),
                DinhKemBaoCao = model.HoSoMoiThauThamDinh?.DinhKemBaoCao?.Select(m => ToDto(m)).ToList()
            }: null,
            ToTrinh = model.ToTrinh != null ? ToUpdateModel(model.ToTrinh) : null,
            QuyetDinh = model.QuyetDinh != null ? ToUpdateModel(model.QuyetDinh) : null,
            DanhSachTepDinhKem = model.DanhSachTepDinhKem?.Select(m => ToDto(m)).ToList()
        };

        return dto;


    }

    public static TepDinhKemModel ToFileModel(this Attachment f) => new() {
        Id = f.Id,
        ParentId = f.ParentId,
        GroupId = f.GroupId,
        GroupType = f.GroupType,
        FileName = f.FileName,
        OriginalName = f.OriginalName,
        Path = f.Path,
        Size = f.Size,
        Type = f.Type,
    };
    public static TepDinhKemDto ToDto(this TepDinhKemModel m) => new() {
        Id = m.Id,
        ParentId = m.ParentId,
        GroupId = m.GroupId,
        GroupType = m.GroupType,
        FileName = m.FileName,
        OriginalName = m.OriginalName,
        Path = m.Path,
        Size = m.Size,
        Type = m.Type,
    };
    public static ToTrinhQuyetDinhDto ToUpdateModel(this ToTrinhQuyetDinhModel model) => new() {
        So = model!.So ?? string.Empty,
        TrichYeu = model!.TrichYeu ?? string.Empty,
        Ngay = model!.Ngay,
        NguoiKy = model!.NguoiKy,
        ChucVu = model!.ChucVu,
        DanhSachTepDinhKem = model?.DanhSachTepDinhKem?.Select(m => ToDto(m)).ToList()
    };
    public static HoSoMoiThauDienTuUpdateModel ToUpdateModel(this HoSoMoiThauDienTuModel model) => new()
    {
        Id = model.GetId(),
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        ThamDinh = model.ThamDinh,
        HinhThucLuaChonNhaThauId = model.HinhThucLuaChonNhaThauId,
        GoiThauId = model.GoiThauId,
        GiaTri = model.GiaTri,
        ThoiGianThucHien = model.ThoiGianThucHien,
        TrangThaiDangTai = model.TrangThaiDangTai,
        TrangThaiId = model.TrangThaiId,
        HoSoMoiThauThamDinh = model.ThamDinh == true && model.HoSoMoiThauThamDinh != null ? new HoSoMoiThauThamDinhDto()
        {
            NhaThauId = model.HoSoMoiThauThamDinh.GetId(),
        
        } : null,
        ToTrinh = model.ToTrinh != null ?   new ToTrinhQuyetDinhDto()
        {
            So = model.ToTrinh!.So,
            TrichYeu = model.ToTrinh!.TrichYeu ?? string.Empty,
            Ngay = model.ToTrinh!.Ngay,
            NguoiKy = model.ToTrinh!.NguoiKy,
            ChucVu = model.ToTrinh!.ChucVu
        }:null,
        QuyetDinh = model.QuyetDinh != null ? new ToTrinhQuyetDinhDto()
        {
            So = model.QuyetDinh!.So ?? string.Empty,
            TrichYeu = model.QuyetDinh!.TrichYeu ?? string.Empty,
            Ngay = model.QuyetDinh!.Ngay,
            NguoiKy = model.QuyetDinh!.NguoiKy,
            ChucVu = model.QuyetDinh!.ChucVu
        } : null
    };
    public static List<Attachment> GetDanhSachTepDinhKemBaoCaoThamDinh(
       this HoSoMoiThauThamDinhModel model, Guid groupId)
       => model.DinhKemBaoCao?.ToEntities(groupId, EGroupType.HoSoMoiThauDienTuBaoCaoTD).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKemCamKetThamDinh(
       this HoSoMoiThauThamDinhModel model, Guid groupId)
       => model.DinhKemCamKet?.ToEntities(groupId, EGroupType.HoSoMoiThauDienTuCamKetTD).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKemQuyetDinhThamDinh(
       this HoSoMoiThauThamDinhModel model, Guid groupId)
       => model.DinhKemQuyetDinh?.ToEntities(groupId, EGroupType.HoSoMoiThauDienTuQuyetDinhTD).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKemToTrinh(
       this ToTrinhQuyetDinhModel model, long groupId)
       => model.DanhSachTepDinhKem?.ToEntities(groupId.ToString(), EGroupType.HoSoMoiThauDienTuToTrinh).ToList() ?? [];
    public static List<Attachment> GetDanhSachTepDinhKemQuyetDinh(
      this ToTrinhQuyetDinhModel model, long groupId)
      => model.DanhSachTepDinhKem?.ToEntities(groupId.ToString(), EGroupType.HoSoMoiThauDienTuQuyetDinh).ToList() ?? [];

    public static List<Attachment> GetDanhSachTepDinhKem(
        this HoSoMoiThauDienTuModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.HoSoMoiThauDienTu).ToList() ?? [];
}
