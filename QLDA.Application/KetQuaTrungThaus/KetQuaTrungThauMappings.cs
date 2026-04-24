using QLDA.Application.KetQuaTrungThaus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.KetQuaTrungThaus;

public static class KetQuaTrungThauMappings {
    public static KetQuaTrungThau ToEntity(this KetQuaTrungThauInsertDto dto) {
        return new KetQuaTrungThau {
            BuocId = dto.BuocId,
            DuAnId = dto.DuAnId,
            GoiThauId = dto.GoiThauId,
            GiaTriTrungThau = dto.GiaTriTrungThau,
            DonViTrungThauId = dto.DonViTrungThauId,
            SoNgayTrienKhai = dto.SoNgayTrienKhai,
            TrichYeu = dto.TrichYeu,
            LoaiGoiThauId = dto.LoaiGoiThauId,
            NgayEHSMT = dto.NgayEHSMT,
            NgayMoThau = dto.NgayMoThau,
            SoQuyetDinh = dto.SoQuyetDinh,
            NgayQuyetDinh = dto.NgayQuyetDinh,
            SoNgayThucHienHopDong = dto.SoNgayThucHienHopDong,
        };
    }

    public static KetQuaTrungThauDto ToDto(this KetQuaTrungThau entity, IEnumerable<TepDinhKem>? files = null) {
        return new KetQuaTrungThauDto {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            GoiThauId = entity.GoiThauId,
            GiaTriTrungThau = entity.GiaTriTrungThau,
            DonViTrungThauId = entity.DonViTrungThauId,
            SoNgayTrienKhai = entity.SoNgayTrienKhai,
            TrichYeu = entity.TrichYeu,
            LoaiGoiThauId = entity.LoaiGoiThauId,
            NgayEHSMT = entity.NgayEHSMT,
            NgayMoThau = entity.NgayMoThau,
            SoQuyetDinh = entity.SoQuyetDinh,
            NgayQuyetDinh = entity.NgayQuyetDinh,
            SoNgayThucHienHopDong = entity.SoNgayThucHienHopDong,
            DanhSachTepDinhKem = [.. files?.ToDtos() ?? []]
        };
    }
    public static void Update(this KetQuaTrungThau entity, KetQuaTrungThauUpdateDto dto) {
        entity.GoiThauId = dto.GoiThauId;
        entity.GiaTriTrungThau = dto.GiaTriTrungThau;
        entity.DonViTrungThauId = dto.DonViTrungThauId;
        entity.SoNgayTrienKhai = dto.SoNgayTrienKhai;
        entity.TrichYeu = dto.TrichYeu;
        entity.LoaiGoiThauId = dto.LoaiGoiThauId;
        entity.NgayEHSMT = dto.NgayEHSMT;
        entity.NgayMoThau = dto.NgayMoThau;
        entity.SoQuyetDinh = dto.SoQuyetDinh;
        entity.NgayQuyetDinh = dto.NgayQuyetDinh;
        entity.SoNgayThucHienHopDong = dto.SoNgayThucHienHopDong;
    }
}