using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.KeHoachLuaChonNhaThauRutGons.DTOs;

namespace QLDA.Application.KeHoachLuaChonNhaThauRutGons;

public static class KeHoachLuaChonNhaThauRutGonMappings
{
    public static KeHoachLuaChonNhaThauRutGon ToEntity(this KeHoachLuaChonNhaThauRutGonDto dto)
    {
        return new KeHoachLuaChonNhaThauRutGon
        {
            Id = dto.Id ?? Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            TrangThaiId = dto.TrangThaiId,
            NhaThauId = dto.NhaThauId,
            GoiThauId = dto.GoiThauId,
            KetQuaDanhGia = dto.KetQuaDanhGia,
        };
    }

    public static KeHoachLuaChonNhaThauRutGonDto ToDto(this KeHoachLuaChonNhaThauRutGon entity, List<Attachment>? files = null)
    {
        return new KeHoachLuaChonNhaThauRutGonDto
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NhaThauId = entity.NhaThauId,
            GoiThauId = entity.GoiThauId,
            KetQuaDanhGia = entity.KetQuaDanhGia,
            TrangThaiId = entity.TrangThaiId,
            
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
        };
    }
}