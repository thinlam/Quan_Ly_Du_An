using QLDA.Application.DuToans.DTOs;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;

namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAnMappings;

public static class KeHoachTrienKhaiChiTietDuAnMappings
{
    public static KeHoachTrienKhaiChiTietDuAn ToEntity(this KeHoachTrienKhaiChiTietDuAnDto dto)
    {
        var entity = new KeHoachTrienKhaiChiTietDuAn()
        {
            Id= dto.Id??Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            GhiChu = dto.GhiChu,
            MaMoc = dto.MaMoc,
            Ten = dto.Ten,
            DonViChuTriId = dto.DonViChuTriId,  
            NgayBatDauKeHoach = dto.NgayBatDauKeHoach,
            NgayBatDauThucTe = dto.NgayBatDauThucTe,
            NgayKetThucKeHoach = dto.NgayKetThucKeHoach,
            NgayKetThucThucTe = dto.NgayKetThucThucTe,
            TiLeHoanThanh = dto.TiLeHoanThanh,
            TrangThaiId = dto.TrangThaiId,
        };

        return entity;
    }


    public static KeHoachTrienKhaiChiTietDuAnDto ToDto(this KeHoachTrienKhaiChiTietDuAn entity, List<TepDinhKem>? files = null) =>
        new()
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,

            TrangThaiId = entity.TrangThaiId,
            Ten = entity.Ten,
            MaMoc = entity.MaMoc,
            GhiChu = entity.GhiChu,
            TiLeHoanThanh = entity.TiLeHoanThanh,
            DonViChuTriId = entity.DonViChuTriId,
            
            NgayBatDauKeHoach = entity.NgayBatDauKeHoach,
            NgayKetThucKeHoach = entity.NgayKetThucKeHoach,
            NgayBatDauThucTe = entity.NgayBatDauThucTe,
            NgayKetThucThucTe = entity.NgayKetThucThucTe,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            

        };
}
