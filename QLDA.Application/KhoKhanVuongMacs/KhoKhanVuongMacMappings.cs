using QLDA.Application.KhoKhanVuongMacs.DTOs;

namespace QLDA.Application.KhoKhanVuongMacs;

public static class KhoKhanVuongMacMappings {
    public static BaoCaoKhoKhanVuongMac ToEntity(this KhoKhanVuongMacInsertDto dto) {
        return new BaoCaoKhoKhanVuongMac {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            Ngay = dto.Ngay,
            NoiDung = dto.NoiDung,
            MucDoKhoKhanId = dto.MucDoKhoKhanId,
            TinhTrangId = dto.TinhTrangId,
            HuongXuLy = dto.HuongXuLy,
            KetQuaXuLy = dto.KetQuaXuLy,
            NgayXuLy = dto.NgayXuLy
        };
    }

    public static BaoCaoKhoKhanVuongMac ToEntity(this KhoKhanVuongMacUpdateDto dto) {
        return new BaoCaoKhoKhanVuongMac {
            Id = dto.Id,
            Ngay = dto.Ngay,
            NoiDung = dto.NoiDung,
            MucDoKhoKhanId = dto.MucDoKhoKhanId,
            TinhTrangId = dto.TinhTrangId,
            HuongXuLy = dto.HuongXuLy,
            KetQuaXuLy = dto.KetQuaXuLy,
            NgayXuLy = dto.NgayXuLy
        };
    }

    public static KhoKhanVuongMacDto ToDto(this BaoCaoKhoKhanVuongMac entity) {
        return new KhoKhanVuongMacDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            Ngay = entity.Ngay.ToDateOnlyVn(),
            NoiDung = entity.NoiDung,
            MucDoKhoKhanId = entity.MucDoKhoKhanId,
            TinhTrangId = entity.TinhTrangId,
            HuongXuLy = entity.HuongXuLy,
            KetQua = new KetQuaXuLyDto {
                KetQuaXuLy = entity.KetQuaXuLy,
                NgayXuLy = entity.NgayXuLy
            }
        };
    }
}
