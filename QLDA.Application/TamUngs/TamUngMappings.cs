using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.TamUngs;

public static class TamUngMappings {
    public static TamUng ToEntity(this TamUngInsertDto dto) {
        return new TamUng {
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            HopDongId = dto.HopDongId,
            SoPhieuChi = dto.SoPhieuChi,
            GiaTri = dto.GiaTri,
            NoiDung = dto.NoiDung,
            NgayTamUng = dto.NgayTamUng,
            SoBaoLanh = dto.SoBaoLanh,
            NgayBaoLanh = dto.NgayBaoLanh,
            NgayKetThucBaoLanh = dto.NgayKetThucBaoLanh
        };
    }

    public static TamUng ToEntity(this TamUngUpdateDto dto) {
        return new TamUng {
            Id = dto.Id,
            HopDongId = dto.HopDongId,
            SoPhieuChi = dto.SoPhieuChi,
            GiaTri = dto.GiaTri, 
            NoiDung = dto.NoiDung,
            NgayTamUng = dto.NgayTamUng,
            SoBaoLanh = dto.SoBaoLanh,
            NgayBaoLanh = dto.NgayBaoLanh,
            NgayKetThucBaoLanh = dto.NgayKetThucBaoLanh
        };
    }

    public static TamUngDto ToDto(this TamUng entity, List<Attachment>? files = null) {
        return new TamUngDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            HopDongId = entity.HopDongId,
            SoPhieuChi = entity.SoPhieuChi,
            GiaTri = entity.GiaTri,
            NoiDung = entity.NoiDung,
            NgayTamUng = entity.NgayTamUng,
            SoBaoLanh = entity.SoBaoLanh,
            NgayBaoLanh = entity.NgayBaoLanh,
            NgayKetThucBaoLanh = entity.NgayKetThucBaoLanh,
            DanhSachTepDinhKem = [.. files?.ToDtos() ?? []]
        };
    }
    public static void Update(this TamUng entity, TamUngUpdateDto dto) {
        entity.HopDongId = dto.HopDongId;
        entity.SoPhieuChi = dto.SoPhieuChi;
        entity.GiaTri = dto.GiaTri;
        entity.NoiDung = dto.NoiDung;
        entity.NgayTamUng = dto.NgayTamUng;
        entity.NgayTamUng = entity.NgayTamUng;
        entity.SoBaoLanh = entity.SoBaoLanh;
        entity.NgayBaoLanh = entity.NgayBaoLanh;
    }
}