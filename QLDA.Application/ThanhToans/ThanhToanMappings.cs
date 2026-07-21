using QLDA.Application.ThanhToans.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ThanhToans;

public static class ThanhToanMappings {
    public static ThanhToan ToEntity(this ThanhToanInsertDto dto) {
        return new ThanhToan {
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            NghiemThuId = dto.NghiemThuId,
            SoHoaDon = dto.SoHoaDon,
            NgayHoaDon = dto.NgayHoaDon,
            GiaTri = dto.GiaTri,
            NoiDung = dto.NoiDung,
            PhuLucHopDongIds = dto.PhuLucs
        };
    }

    public static ThanhToan ToEntity(this ThanhToanUpdateDto dto) {
        return new ThanhToan {
            Id = dto.Id,
            NghiemThuId = dto.NghiemThuId,
            SoHoaDon = dto.SoHoaDon,
            NgayHoaDon = dto.NgayHoaDon,
            GiaTri = dto.GiaTri,
            NoiDung = dto.NoiDung,
            PhuLucHopDongIds = dto.PhuLucs
        };
    }

    public static ThanhToanDto ToDto(this ThanhToan entity, IEnumerable<Attachment>? files = null) {
        return new ThanhToanDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NghiemThuId = entity.NghiemThuId,
            SoHoaDon = entity.SoHoaDon,
            NgayHoaDon = entity.NgayHoaDon,
            GiaTri = entity.GiaTri,
            NoiDung = entity.NoiDung,
            PhuLucHopDongIds= entity.PhuLucHopDongIds,
            DanhSachTepDinhKem = [.. files?.ToDtos() ?? []]
        };
    }
    public static void Update(this ThanhToan entity, ThanhToanUpdateDto dto) {
        entity.NghiemThuId = dto.NghiemThuId;
        entity.SoHoaDon = dto.SoHoaDon;
        entity.NgayHoaDon = dto.NgayHoaDon;
        entity.GiaTri = dto.GiaTri;
        entity.PhuLucHopDongIds = dto.PhuLucs;
        entity.NoiDung = dto.NoiDung;
    }
}