using Microsoft.EntityFrameworkCore;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Application.TrienKhaiKeHoachLCNTs.DTOs;

namespace QLDA.Application.TrienKhaiKeHoachLCNTMappings;

public static class TrienKhaiKeHoachLCNTMappings {
    public static async Task SyncDonViTuVanAsync(
        this DbContext dbContext,
        Guid keHoachId,
        List<DonViTuVanKeHoach>? donVis,
        CancellationToken cancellationToken = default) {
        var requestList = donVis ?? [];
        var requestIds = requestList.Select(d => d.Id).ToHashSet();

        var existingInDb = await dbContext.Set<DonViTuVanKeHoach>()
            .Where(d => d.KeHoachId == keHoachId)
            .ToListAsync(cancellationToken);

        foreach (var toRemove in existingInDb.Where(d => !requestIds.Contains(d.Id)).ToList())
            dbContext.Set<DonViTuVanKeHoach>().Remove(toRemove);

        foreach (var item in requestList) {
            var existing = existingInDb.FirstOrDefault(d => d.Id == item.Id);
            if (existing is not null) {
                existing.TenDonVi = item.TenDonVi;
                existing.KeHoachId = keHoachId;
            } else {
                await dbContext.Set<DonViTuVanKeHoach>().AddAsync(new DonViTuVanKeHoach {
                    Id = item.Id,
                    KeHoachId = keHoachId,
                    TenDonVi = item.TenDonVi
                }, cancellationToken);
            }
        }
    }

    public static TrienKhaiKeHoachLCNTDto ToDto(this TrienKhaiKeHoachLCNT entity, List<Attachment>? files = null) =>
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
