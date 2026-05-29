using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Application.TrienKhaiKeHoachLCNTs.DTOs;

namespace QLDA.Application.TrienKhaiKeHoachLCNTMappings;

public static class TrienKhaiKeHoachLCNTMappings {
    public static void SyncDonViTuVan(this TrienKhaiKeHoachLCNT entity, List<DonViTuVanKeHoach>? donVis) {
        if (donVis is null) {
            entity.DonViTuVans = [];
            return;
        }

        entity.DonViTuVans ??= [];
        entity.DonViTuVans.Clear();
        foreach (var item in donVis) {
            entity.DonViTuVans.Add(new DonViTuVanKeHoach {
                Id= item.Id,
                KeHoachId = entity.Id,
                TenDonVi = item.TenDonVi
            });
        }
    }

    public static TrienKhaiKeHoachLCNTDto ToDto(this TrienKhaiKeHoachLCNT entity, List<TepDinhKem>? files = null) =>
        new() {
            Id = entity.Id,
            So = entity.So,
            NgayTrinh = entity.NgayTrinh,
            TrichYeu = entity.TrichYeu,
            TrangThaiId = entity.TrangThaiId,
            TrangThaiDangTaiId = entity.TrangThaiDangTaiId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            DanhSachDonViTuVan = entity.DonViTuVans?
                                .Select(x => new DonViTuVanKeHoachDto
                                {
                                    Id = x.Id,
                                    KeHoachId = x.KeHoachId,
                                    TenDonVi = x.TenDonVi,
                                })
                                .ToList(),

        };
}
