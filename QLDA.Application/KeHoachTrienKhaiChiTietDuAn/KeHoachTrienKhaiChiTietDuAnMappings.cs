using QLDA.Application.DuToans.DTOs;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAnMappings;

public static class KeHoachTrienKhaiChiTietDuAnMappings {
    public static void ToEntity( this KeHoachTrienKhaiChiTietDuAnDto dto, KeHoachTrienKhaiChiTietDuAn entity) {
        entity.Id = dto.Id ?? Guid.NewGuid();
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        entity.GhiChu = dto.GhiChu;
        entity.MaMoc = dto.MaMoc;
        entity.Ten = dto.Ten;
        entity.DonViChuTriId = dto.DonViChuTriId;
        entity.NgayBatDauKeHoach = dto.NgayBatDauKeHoach;
        entity.NgayBatDauThucTe = dto.NgayBatDauThucTe;
        entity.NgayKetThucKeHoach = dto.NgayKetThucKeHoach;
        entity.NgayKetThucThucTe = dto.NgayKetThucThucTe;
        entity.TiLeHoanThanh = dto.TiLeHoanThanh;
        entity.TrangThaiId = dto.TrangThaiId;
    }



    public static KeHoachTrienKhaiChiTietDuAnDto ToDto(this KeHoachTrienKhaiChiTietDuAn entity, List<Attachment>? files = null) =>
        new() {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,

            TrangThaiId = entity.TrangThaiId,
            Ten = entity.Ten ?? string.Empty,
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
