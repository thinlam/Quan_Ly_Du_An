using QLDA.Application.Common.Constants;

namespace QLDA.Application.DanhMucBuocs.DTOs;

public static class DanhMucBuocMappingConfiguration {
    public static DanhMucBuocDto ToDto(this DanhMucBuoc entity)
        => new DanhMucBuocDto() {
            Id = entity.Id,
            ParentId = entity.ParentId,
            GiaiDoanId = entity.GiaiDoanId,
            Level = entity.Level,
            Ma = entity.Ma ?? string.Empty,
            MoTa = entity.MoTa ?? string.Empty,
            Path = entity.Path,
            QuyTrinhId = entity.QuyTrinhId,
            SoNgayThucHien = entity.SoNgayThucHien,
            Stt = entity.Stt,
            Used = entity.Used,
            Ten = entity.Ten ?? string.Empty,
            DanhSachManHinh = entity.BuocManHinhs?.OrderByDefault().Select(e => e.RightId).ToList(),
        };

    public static List<DanhMucBuocDto> ToDtos(this List<DanhMucBuoc> entities)
        => [.. entities.Select(e => e.ToDto())];

    public static DanhMucBuocDto ToDto(this DanhMucBuocMaterializedDto dto)
        => new DanhMucBuocDto() {
            Id = dto.Entity.Id,
            Ma = dto.Entity.Ma,
            Ten = dto.Entity.Ten,
            MoTa = dto.Entity.MoTa,
            Used = dto.Entity.Used,
            QuyTrinhId = dto.Entity.QuyTrinhId,
            GiaiDoanId = dto.Entity.GiaiDoanId,
            SoNgayThucHien = dto.Entity.SoNgayThucHien,
            PartialView = dto.Entity.PartialView,
            DanhSachManHinh = dto.Entity.BuocManHinhs?.OrderByDefault().Select(e => e.RightId).ToList(),
            #region Materialized Path 
            Stt = dto.Entity.Stt,
            ParentId = dto.Entity.ParentId,
            Level = dto.Entity.Level,
            Path = dto.Entity.Path,
            Ancestors = dto.Ancestors?.ToDtos(), //Tổ tiên
            Descendants = dto.Descendants?.ToDtos(), //Hậu duệ
            #endregion
        };
}