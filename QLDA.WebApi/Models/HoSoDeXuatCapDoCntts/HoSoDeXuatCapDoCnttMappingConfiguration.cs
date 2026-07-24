using BuildingBlocks.Domain.Entities;
using QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoDeXuatCapDoCntts;

public static class HoSoDeXuatCapDoCnttMappingConfiguration {
    
    public static HoSoDeXuatCapDoCnttModel ToModel(
        this HoSoDeXuatCapDoCntt entity,
        List<Attachment>? files = null) => new() {
        
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        TrangThaiId = entity.TrangThaiId,
        CapDoId = entity.CapDoId,
        NgayTrinh = entity.NgayTrinh.ToDateOnlyVn(),
        DonViChuTriId = entity.DonViChuTriId,
        NoiDungDeNghi = entity.NoiDungDeNghi,
        NoiDungBaoCao = entity.NoiDungBaoCao,
        NoiDungDuThao = entity.NoiDungDuThao,
        DanhSachTepDinhKem = files?.Select(o => new TepDinhKemModel {
            Id = o.Id,
            ParentId = o.ParentId,
            GroupId = o.GroupId,
            GroupType = o.GroupType,
            Path = o.Path,
            Size = o.Size,
            Type = o.Type,
            FileName = o.FileName,
            OriginalName = o.OriginalName
        }).ToList()
    };

    public static HoSoDeXuatCapDoCnttInsertDto ToInsertDto(this HoSoDeXuatCapDoCnttModel model) => new() {
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        TrangThaiId = model.TrangThaiId,
        CapDoId = model.CapDoId,
        NgayTrinh = model.NgayTrinh,
        DonViChuTriId = model.DonViChuTriId,
        NoiDungDeNghi = model.NoiDungDeNghi,
        NoiDungBaoCao = model.NoiDungBaoCao,
        NoiDungDuThao = model.NoiDungDuThao,
        DanhSachTepDinhKem = model.DanhSachTepDinhKem?.Select(m => new TepDinhKemDto {
            Id = m.Id,
            ParentId = m.ParentId,
            GroupId = m.GroupId,
            GroupType = m.GroupType,
            Path = m.Path,
            Size = m.Size,
            Type = m.Type,
            FileName = m.FileName,
            OriginalName = m.OriginalName
        }).ToList()
    };

    public static HoSoDeXuatCapDoCnttUpdateModel ToUpdateModel(this HoSoDeXuatCapDoCnttModel model) => new() {
        Id = model.GetId(),
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        TrangThaiId = model.TrangThaiId,
        CapDoId = model.CapDoId,
        NgayTrinh = model.NgayTrinh,
        DonViChuTriId = model.DonViChuTriId,
        NoiDungDeNghi = model.NoiDungDeNghi,
        NoiDungBaoCao = model.NoiDungBaoCao,
        NoiDungDuThao = model.NoiDungDuThao
    };

    public static List<Attachment> GetDanhSachTepDinhKem(
        this HoSoDeXuatCapDoCnttModel model, Guid groupId)
        => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.HoSoDeXuatCapDoCntt).ToList() ?? [];
}
