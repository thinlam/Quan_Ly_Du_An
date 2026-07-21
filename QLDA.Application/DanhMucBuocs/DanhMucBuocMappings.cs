using QLDA.Application.DanhMucBuocs.DTOs;

namespace QLDA.Application.DanhMucBuocs;

public static class DanhMucBuocMappings {
    public static DanhMucBuoc ToEntity(this DanhMucBuocInsertDto dto) {
        return new DanhMucBuoc {
            Ma = dto.Ma,
            Ten = dto.Ten,
            MoTa = dto.MoTa,
            Stt = dto.Stt,
            Used = dto.Used,
            QuyTrinhId = dto.QuyTrinhId,
            GiaiDoanId = dto.GiaiDoanId,
            ParentId = dto.ParentId,
            SoNgayThucHien = dto.SoNgayThucHien,
            BuocManHinhs = [ ..dto.DanhSachManHinh?.Select((manHinhId, index) => new DanhMucBuocManHinh() {
                RightId = manHinhId,
                Stt = index + 1
            }) ?? []]
        };
    }
    public static void Update(this DanhMucBuoc entity, DanhMucBuocUpdateDto dto) {
        entity.Ma = dto.Ma;
        entity.Ten = dto.Ten;
        entity.MoTa = dto.MoTa;
        entity.Stt = dto.Stt;
        entity.Used = dto.Used;
        entity.QuyTrinhId = dto.QuyTrinhId;
        entity.GiaiDoanId = dto.GiaiDoanId;
        entity.ParentId = dto.ParentId;
        entity.SoNgayThucHien = dto.SoNgayThucHien;
        if (dto.DanhSachManHinh?.Count > 0) {
            entity.BuocManHinhs = [ ..dto.DanhSachManHinh.Select((manHinhId, index) => new DanhMucBuocManHinh() {
                LeftId = entity.Id,
                RightId = manHinhId,
                Stt = index + 1
            })];
        }
    }

    public static DanhMucBuocDto ToDto(this DanhMucBuoc entity) {
        return new DanhMucBuocDto {
            Id = entity.Id,
            Ma = entity.Ma ?? string.Empty,
            Ten = entity.Ten ?? string.Empty,
            MoTa = entity.MoTa ?? string.Empty,
            Stt = entity.Stt,
            Used = entity.Used,
            QuyTrinhId = entity.QuyTrinhId,
            GiaiDoanId = entity.GiaiDoanId,
            ParentId = entity.ParentId,
            SoNgayThucHien = entity.SoNgayThucHien
        };
    }
}