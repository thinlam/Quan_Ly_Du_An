using QLDA.Application.Common.Constants;
using QLDA.Application.DanhMucBuocs.DTOs;
using QLDA.Application.DuAnBuocs.DTOs;

namespace QLDA.Application.DuAnBuocs;

public static class DuAnBuocMapping {
    public static DuAnBuocDto ToDto(this DuAnBuoc entity)
            => new() {
                Id = entity.Id,
                TenBuoc = entity.TenBuoc ?? entity.Buoc?.Ten,
                GiaiDoanId = entity.Buoc?.GiaiDoanId,
                BuocId = entity.BuocId,
                DuAnId = entity.DuAnId,
                ParentId = entity.Buoc?.ParentId,
                Stt = entity.Buoc?.Stt ?? 0,
                Used = entity.Used,
                Path = entity.Buoc?.Path ?? $"/{entity.BuocId}/",
                NgayDuKienBatDau = entity.NgayDuKienBatDau,
                NgayDuKienKetThuc = entity.NgayDuKienKetThuc,
                GiaiDoan = entity.Buoc?.GiaiDoan?.ToPhaseDto(),
                DanhSachManHinh = entity.DuAnBuocManHinhs is { Count: > 0 }
                    ? entity.DuAnBuocManHinhs.OrderByDefault().Select(e => e.RightId).ToList()
                    : entity.Buoc?.BuocManHinhs?.OrderByDefault().Select(e => e.RightId).ToList() ?? [],
                PhongPhuTrachChinhId = entity.PhongPhuTrachChinhId,
                DanhSachPhongBanPhoiHopIds = entity.DuAnBuocPhongBanPhoiHops?.Select(o => o.RightId).ToList() ?? [],
            };

    public static DuAnBuocDuAnUpdateStateDto ToUpdateStateDto(this DuAnBuoc entity)
        => new() {
            Id = entity.Id,
            TrangThaiId = entity.TrangThaiId,
            NgayDuKienBatDau = entity.NgayDuKienBatDau.ToDateOnlyVn(),
            NgayDuKienKetThuc = entity.NgayDuKienKetThuc.ToDateOnlyVn(),
            NgayThucTeBatDau = entity.NgayThucTeBatDau.ToDateOnlyVn(),
            NgayThucTeKetThuc = entity.NgayThucTeKetThuc.ToDateOnlyVn(),
            GhiChu = entity.GhiChu,
            TrachNhiemThucHien = entity.TrachNhiemThucHien,
            IsKetThuc = entity.IsKetThuc,
            PhongPhuTrachChinhId = entity.PhongPhuTrachChinhId,
            DanhSachPhongBanPhoiHopIds = entity.DuAnBuocPhongBanPhoiHops?
                .OrderBy(p => p.RightId)
                .Select(p => p.RightId)
                .ToList() ?? []
        };

    public static void UpdateState(this DuAnBuoc entity, DuAnBuocDuAnUpdateStateDto dto) {
        entity.TrangThaiId = dto.TrangThaiId;
        entity.NgayDuKienBatDau = dto.NgayDuKienBatDau?.ToStartOfDayUtc();
        entity.NgayDuKienKetThuc = dto.NgayDuKienKetThuc?.ToEndOfDayUtc();
        entity.NgayThucTeBatDau = dto.NgayThucTeBatDau?.ToStartOfDayUtc();
        entity.NgayThucTeKetThuc = dto.NgayThucTeKetThuc?.ToEndOfDayUtc();
        entity.GhiChu = dto.GhiChu;
        entity.TrachNhiemThucHien = dto.TrachNhiemThucHien;
        entity.IsKetThuc = dto.IsKetThuc;
        entity.PhongPhuTrachChinhId = dto.PhongPhuTrachChinhId;
    }

    public static void Update(this DuAnBuoc entity, DuAnBuocUpdateDto dto) {
        entity.TenBuoc = dto.TenBuoc;
        entity.Used = dto.Used;
        entity.NgayDuKienBatDau = dto.NgayDuKienBatDau?.ToStartOfDayUtc();
        entity.NgayDuKienKetThuc = dto.NgayDuKienKetThuc?.ToEndOfDayUtc();

        // Proper sync for DuAnBuocManHinhs instead of replacing collection
        if (dto.DanhSachManHinh?.Count > 0) {
            var existingManHinhs = entity.DuAnBuocManHinhs?.ToList() ?? [];
            var requestRightIds = dto.DanhSachManHinh.ToHashSet();
            var existingByKey = existingManHinhs.ToDictionary(m => m.RightId, m => m);

            // Remove entities not in request
            foreach (var existingMh in existingManHinhs.Where(m => !requestRightIds.Contains(m.RightId))) {
                entity.DuAnBuocManHinhs?.Remove(existingMh);
            }

            // Add new entities or update existing Stt
            foreach (var (rightId, index) in dto.DanhSachManHinh.Select((id, i) => (id, i))) {
                if (existingByKey.TryGetValue(rightId, out var existingMh)) {
                    existingMh.Stt = index + 1;
                } else {
                    entity.DuAnBuocManHinhs?.Add(new DuAnBuocManHinh {
                        RightId = rightId,
                        Stt = index + 1
                    });
                }
            }
        }
    }
}
