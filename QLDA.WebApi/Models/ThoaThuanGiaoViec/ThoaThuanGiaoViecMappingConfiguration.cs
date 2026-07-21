using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.ThoaThuanGiaoViecs;

namespace QLDA.WebApi.Models.ThoaThuanGiaoViecs;

public static class ThoaThuanGiaoViecMappingConfiguration
{
    public static ThoaThuanGiaoViecModel ToModel(this ThoaThuanGiaoViec entity, List<Attachment>? danhSachTepDinhKem = null) =>
        new()
        {
            Id = entity.Id,
            GoiThauId = entity.GoiThauId,
            ChatLuong = entity.ChatLuong,
            NoiDung = entity.NoiDung,
            PhamVi = entity.PhamVi,
            ThoiGian = entity.ThoiGian,
            GiaTri = entity.GiaTri,

            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = danhSachTepDinhKem?.Select(o => o.ToModel()).ToList(),
        };


   

   
}
