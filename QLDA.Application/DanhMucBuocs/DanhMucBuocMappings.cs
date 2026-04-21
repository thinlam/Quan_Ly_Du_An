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
                ManHinhId = manHinhId,
                Stt = index  // Lưu vị trí theo thứ tự từ request
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
        entity.BuocManHinhs = [ ..dto.DanhSachManHinh?.Select((manHinhId, index) => new DanhMucBuocManHinh() {
            BuocId = entity.Id,
            ManHinhId = manHinhId,
            Stt = index  // Lưu vị trí theo thứ tự từ request
        }) ?? []];
    }

    public static DanhMucBuocDto ToDto(this DanhMucBuoc entity) {
        return new DanhMucBuocDto {
            Id = entity.Id,
            Ma = entity.Ma,
            Ten = entity.Ten,
            MoTa = entity.MoTa,
            Stt = entity.Stt,
            Used = entity.Used,
            QuyTrinhId = entity.QuyTrinhId,
            GiaiDoanId = entity.GiaiDoanId,
            ParentId = entity.ParentId,
            SoNgayThucHien = entity.SoNgayThucHien
        };
    }
}