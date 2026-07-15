using QLDA.Application.QuyetDinhLapBenMoiThaus.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.QuyetDinhLapBenMoiThaus;

public static class QuyetDinhLapBenMoiThauMappings {
    public static QuyetDinhLapBenMoiThau ToEntity(this QuyetDinhLapBenMoiThauInsertDto dto) {
        return new QuyetDinhLapBenMoiThau {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.SoQuyetDinh,
            Ngay = dto.NgayQuyetDinh,
            TrichYeu = dto.TrichYeu,
            NoiDung = dto.NoiDung,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy,
            Loai = EnumLoaiVanBanQuyetDinh.QuyetDinhLapBenMoiThau.ToString(),

        };
    }

    public static QuyetDinhLapBenMoiThau ToEntity(this QuyetDinhLapBenMoiThauUpdateDto dto) {
        return new QuyetDinhLapBenMoiThau {
            Id = dto.Id,
            So = dto.SoQuyetDinh,
            Ngay = dto.NgayQuyetDinh,
            TrichYeu = dto.TrichYeu,
            NoiDung = dto.NoiDung,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy,
            
            Loai = EnumLoaiVanBanQuyetDinh.QuyetDinhLapBenMoiThau.ToString(),

        };
    }

    public static QuyetDinhLapBenMoiThauDto ToDto(this QuyetDinhLapBenMoiThau entity) {
        return new QuyetDinhLapBenMoiThauDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            SoQuyetDinh = entity.So,
            NgayQuyetDinh = entity.Ngay,
            TrichYeu = entity.TrichYeu,
            NoiDung = entity.NoiDung,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy


        };
    }
}