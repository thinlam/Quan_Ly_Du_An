using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ThoaThuanGiaoViecs.DTOs;

namespace QLDA.Application.ThoaThuanGiaoViecs;

public static class ThoaThuanGiaoViecMappings
{

    public static ThoaThuanGiaoViecDto ToDto(this ThoaThuanGiaoViec entity, List<TepDinhKem>? files = null)
    {
        return new ThoaThuanGiaoViecDto
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            TrangThaiId = entity.TrangThaiId,

            PhamVi = entity.PhamVi,
            ChatLuong = entity.ChatLuong,
            GoiThauId = entity.GoiThauId,
            GiaTri = entity.GiaTri,
            NoiDung = entity.NoiDung,
            ThoiGian = entity.ThoiGian,

            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
        };
    }
}