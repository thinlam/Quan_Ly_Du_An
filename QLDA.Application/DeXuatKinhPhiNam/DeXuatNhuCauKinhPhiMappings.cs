using QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNamMappings;

public static class DeXuatNhuCauKinhPhiNamMappings {
    public static void SyncDeXuatIds(this DeXuatNhuCauKinhPhiNam entity, List<Guid>? deXuatIds) {
        if (deXuatIds is null) {
            entity.DeXuats = [];
            return;
        }

        entity.DeXuats ??= [];
        entity.DeXuats.Clear();
        foreach (var deXuatId in deXuatIds) {
            entity.DeXuats.Add(new DeXuatTrinhKinhPhiNam {
                LeftId = entity.Id,
                RightId = deXuatId,
            });
        }
    }

    public static DeXuatNhuCauKinhPhiNamDto ToDto(this DeXuatNhuCauKinhPhiNam entity, List<TepDinhKem>? files = null) =>
        new() {
            Id = entity.Id,
            So = entity.So,
            GhiChu = entity.GhiChu,
            NgayKeHoach = entity.NgayKeHoach,
            TrichYeu = entity.TrichYeu,
            TongKinhPhiDeXuat = entity.TongKinhPhiDeXuat,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            DanhSachDeXuat = entity.DeXuats?.Select(x => x.RightId).ToList(),
        };
}
