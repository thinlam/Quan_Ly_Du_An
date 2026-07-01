using QLDA.Application.QuyetDinhLapHoiDongThamDinhs.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.QuyetDinhLapHoiDongThamDinhs;

public static class QuyetDinhLapHoiDongThamDinhMappings {
    public static QuyetDinhLapHoiDongThamDinh ToEntity(this QuyetDinhLapHoiDongThamDinhInsertDto dto) {
        return new QuyetDinhLapHoiDongThamDinh {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.SoQuyetDinh,
            Ngay = dto.NgayQuyetDinh??dto.NgayKy,//chi co ngay ky
            TrichYeu = dto.TrichYeu,
            CoQuanQuyetDinh = dto.CoQuanQuyetDinh,
            NoiDung = dto.NoiDung,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy,
            Loai = EnumLoaiVanBanQuyetDinh.QuyetDinhLapHoiDongThamDinh.ToString(),
        };
    }

    public static QuyetDinhLapHoiDongThamDinh ToEntity(this QuyetDinhLapHoiDongThamDinhUpdateDto dto) {
        return new QuyetDinhLapHoiDongThamDinh {
            Id = dto.Id,
            So = dto.SoQuyetDinh,
            Ngay = dto.NgayQuyetDinh ?? dto.NgayKy,
            TrichYeu = dto.TrichYeu,
            NoiDung = dto.NoiDung,
            CoQuanQuyetDinh = dto.CoQuanQuyetDinh,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy,
            Loai = EnumLoaiVanBanQuyetDinh.QuyetDinhLapHoiDongThamDinh.ToString(),
        };
    }

    public static QuyetDinhLapHoiDongThamDinhDto ToDto(this QuyetDinhLapHoiDongThamDinh entity) {
        return new QuyetDinhLapHoiDongThamDinhDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            SoQuyetDinh = entity.So,
            NgayQuyetDinh = entity.Ngay,
            TrichYeu = entity.TrichYeu,
            CoQuanQuyetDinh = entity.CoQuanQuyetDinh,
            NoiDung = entity.NoiDung,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy
        };
    }
}