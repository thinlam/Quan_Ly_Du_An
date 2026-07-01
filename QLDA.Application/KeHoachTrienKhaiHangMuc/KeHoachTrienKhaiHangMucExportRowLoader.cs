using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs;

/// <summary>
/// Map danh sách HangMucKeHoach → export rows (group theo giai đoạn).
/// Dùng chung cho Excel export và Word phiếu trình.
/// </summary>
internal static class KeHoachTrienKhaiHangMucExportRowLoader
{
    public static async Task<List<KeHoachTrienKhaiHangMucExportItemDto>> LoadAsync(
        IReadOnlyList<HangMucKeHoach> hangMucs,
        IRepository<DanhMucGiaiDoan, int> giaiDoanRepo,
        IRepository<DmDonVi, long> donViRepo,
        IRepository<UserMaster, long> userRepo,
        CancellationToken cancellationToken = default)
    {
        if (hangMucs.Count == 0)
            return [];

        var giaiDoanIds = hangMucs
            .Where(h => h.GiaiDoanId.HasValue)
            .Select(h => h.GiaiDoanId!.Value)
            .Distinct()
            .ToList();

        var giaiDoans = giaiDoanIds.Count == 0
            ? []
            : await giaiDoanRepo.GetQueryableSet()
                .AsNoTracking()
                .Where(g => giaiDoanIds.Contains(g.Id))
                .ToListAsync(cancellationToken);

        var donViIds = hangMucs
            .SelectMany(h => Enumerable.Empty<long?>()
                .Append(h.DonViChuTriId)
                .Concat((h.DonViPhoiHopIds ?? []).Select(id => (long?)id)))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var donVis = donViIds.Count == 0
            ? []
            : await donViRepo.GetQueryableSet()
                .AsNoTracking()
                .Where(d => donViIds.Contains(d.Id))
                .Select(d => new { d.Id, d.TenDonVi })
                .ToListAsync(cancellationToken);

        var userIds = hangMucs
            .SelectMany(h => Enumerable.Empty<long?>()
                .Append(h.CanBoChuTriId)
                .Concat((h.CanBoPhoiHopIds ?? []).Select(id => (long?)id)))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var users = userIds.Count == 0
            ? []
            : await userRepo.GetQueryableSet()
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.HoTen })
                .ToListAsync(cancellationToken);

        return KeHoachTrienKhaiHangMucExportMapper.ToExportRows(
            hangMucs,
            giaiDoans.ToDictionary(g => g.Id, g => g.Ten ?? string.Empty),
            giaiDoans.ToDictionary(g => g.Id, g => g.Stt ?? int.MaxValue - 1),
            donVis.ToDictionary(d => d.Id, d => d.TenDonVi ?? string.Empty),
            users.ToDictionary(u => u.Id, u => u.HoTen ?? string.Empty));
    }
}
