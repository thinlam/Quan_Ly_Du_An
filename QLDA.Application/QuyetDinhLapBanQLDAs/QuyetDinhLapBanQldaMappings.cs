using QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.QuyetDinhLapBanQLDAs;

public static class QuyetDinhLapBanQldaMappings {
    public static QuyetDinhLapBanQLDA ToEntity(this QuyetDinhLapBanQldaInsertDto dto) {
        var entity = new QuyetDinhLapBanQLDA {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.SoQuyetDinh,
            Ngay = dto.NgayQuyetDinh,
            TrichYeu = dto.TrichYeu,
            CoQuanQuyetDinh = dto.CoQuanQuyetDinh,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy,
            Loai = EnumLoaiVanBanQuyetDinh.QuyetDinhLapBanQLDA.ToString(),

        };

        if (dto.DanhSachThanhVien != null) {
            entity.ThanhViens = [.. dto.DanhSachThanhVien.Select(tv => new ThanhVienBanQLDA {
                Id = tv.Id ?? 0,
                Ten = tv.Ten,
                ChucVu = tv.ChucVu,
                VaiTro = tv.VaiTro
            })];
        }

        return entity;
    }

    public static QuyetDinhLapBanQLDA ToEntity(this QuyetDinhLapBanQldaUpdateDto dto) {
        var entity = new QuyetDinhLapBanQLDA {
            Id = dto.Id,
            So = dto.SoQuyetDinh,
            Ngay = dto.NgayQuyetDinh,
            CoQuanQuyetDinh = dto.CoQuanQuyetDinh,
            TrichYeu = dto.TrichYeu,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy,
            Loai = EnumLoaiVanBanQuyetDinh.QuyetDinhLapBanQLDA.ToString(),
        };

        if (dto.DanhSachThanhVien != null) {
            entity.ThanhViens = [.. dto.DanhSachThanhVien.Select(tv => new ThanhVienBanQLDA {
                Id = tv.Id ?? 0,
                Ten = tv.Ten,
                ChucVu = tv.ChucVu,
                VaiTro = tv.VaiTro
            })];
        }

        return entity;
    }

    public static QuyetDinhLapBanQldaDto ToDto(this QuyetDinhLapBanQLDA entity) {
        return new QuyetDinhLapBanQldaDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            SoQuyetDinh = entity.So,
            CoQuanQuyetDinh = entity.CoQuanQuyetDinh,
            NgayQuyetDinh = entity.Ngay,
            TrichYeu = entity.TrichYeu,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy
        };
    }
}