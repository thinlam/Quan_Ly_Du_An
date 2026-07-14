using QLDA.Application.KeHoachLuaChonNhaThaus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.KeHoachLuaChonNhaThaus;

public static class KeHoachLuaChonNhaThauMappings {
    public static KeHoachLuaChonNhaThau ToEntity(this KeHoachLuaChonNhaThauInsertDto dto) {
        return new KeHoachLuaChonNhaThau {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            Ten = dto.Ten,
            So = dto.SoQuyetDinh,
            Ngay = dto.NgayQuyetDinh,
            TrichYeu = dto.TrichYeu,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy
        };
    }

    public static KeHoachLuaChonNhaThauDto ToDto(this KeHoachLuaChonNhaThau entity, IEnumerable<Attachment>? files = null) {
        return new KeHoachLuaChonNhaThauDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            Ten = entity.Ten ?? string.Empty,
            SoQuyetDinh = entity.So,
            NgayQuyetDinh = entity.Ngay,
            TrichYeu = entity.TrichYeu,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy,
            DanhSachTepDinhKem = [.. files?.ToDtos() ?? []]
        };
    }
    public static void Update(this KeHoachLuaChonNhaThau entity, KeHoachLuaChonNhaThauUpdateDto dto) {
        entity.Ten = dto.Ten;
        entity.So = dto.SoQuyetDinh;
        entity.Ngay = dto.NgayQuyetDinh;
        entity.TrichYeu = dto.TrichYeu;
        entity.NgayKy = dto.NgayKy;
        entity.NguoiKy = dto.NguoiKy;
    }
}