using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs;

/// <summary>
/// Map <see cref="HangMucKeHoach"/> → export rows (group theo giai đoạn).
/// Dùng chung cho Excel export và Word phiếu trình.
/// </summary>
internal static class KeHoachTrienKhaiHangMucExportMappings
{
    private const string KhacGiaiDoanTen = "Khác";

    public static async Task<List<KeHoachTrienKhaiHangMucExportItemDto>> ToExportRowsAsync(
        IReadOnlyList<HangMucKeHoach> hangMucs,
        Guid? duAnId,
        IRepository<DanhMucGiaiDoan, int> giaiDoanRepo,
        IRepository<DuAnBuoc, int> duAnBuocRepo,
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

        Dictionary<int, int> giaiDoanSortById;

        if (duAnId is Guid projectId && projectId != Guid.Empty)
        {
            giaiDoanSortById = new Dictionary<int, int>(
                await KeHoachTrienKhaiHangMucImportGiaiDoanHelper.GetGiaiDoanSortByDuAnAsync(
                    projectId, duAnBuocRepo, giaiDoanRepo, cancellationToken));

            foreach (var id in giaiDoanIds.Where(id => !giaiDoanSortById.ContainsKey(id)))
                giaiDoanSortById[id] = int.MaxValue - 1;
        }
        else
        {
            giaiDoanSortById = giaiDoans.ToDictionary(g => g.Id, g => g.Stt ?? int.MaxValue - 1);
        }

        var donViIds = hangMucs
            .SelectMany(h => Enumerable.Empty<long?>()
                .Append(h.DonViChuTriId)
                .Concat((h.DonViPhoiHopIds ?? []).Select(id => (long?)id)))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        // Legacy DmDonVi / UserMaster: Used có thể null — không lọc OnlyUsed.
        var donVis = donViIds.Count == 0
            ? []
            : await donViRepo.GetQueryableSet(OnlyUsed: false, OnlyNotDeleted: false)
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

        // CanBoChuTriId / CanBoPhoiHopIds store UserPortalId (same as UI combobox).
        var users = userIds.Count == 0
            ? []
            : await userRepo.GetQueryableSet(OnlyUsed: false, OnlyNotDeleted: false)
                .AsNoTracking()
                .Where(u => u.UserPortalId.HasValue && userIds.Contains(u.UserPortalId.Value))
                .Select(u => new { PortalId = u.UserPortalId!.Value, u.HoTen })
                .ToListAsync(cancellationToken);

        var giaiDoanTenById = giaiDoans.ToDictionary(g => g.Id, g => g.Ten ?? string.Empty);

        if (duAnId is Guid duAnIdValue && duAnIdValue != Guid.Empty)
        {
            var displayNameById = await KeHoachTrienKhaiHangMucImportGiaiDoanHelper
                .GetGiaiDoanDisplayNameByDuAnAsync(
                    duAnIdValue, duAnBuocRepo, giaiDoanRepo, cancellationToken);

            foreach (var (giaiDoanId, displayName) in displayNameById)
            {
                if (giaiDoanIds.Contains(giaiDoanId) && !string.IsNullOrWhiteSpace(displayName))
                    giaiDoanTenById[giaiDoanId] = displayName;
            }
        }

        return ToExportRows(
            hangMucs,
            giaiDoanTenById,
            giaiDoanSortById,
            donVis.ToDictionary(d => d.Id, d => d.TenDonVi ?? string.Empty),
            users.ToDictionary(u => u.PortalId, u => u.HoTen ?? string.Empty));
    }

    public static List<KeHoachTrienKhaiHangMucExportItemDto> ToExportRows(
        IEnumerable<HangMucKeHoach> hangMucs,
        IReadOnlyDictionary<int, string> giaiDoanTenById,
        IReadOnlyDictionary<int, int> giaiDoanSortById,
        IReadOnlyDictionary<long, string> donViTenById,
        IReadOnlyDictionary<long, string> userTenById)
    {
        var items = hangMucs.ToList();
        if (items.Count == 0)
            return [];

        var itemIndexById = items
            .Select((h, i) => (h.Id, i))
            .ToDictionary(x => x.Id, x => x.i);

        var firstGroupIndexByGiaiDoanId = items
            .Select((h, i) => (h.GiaiDoanId, i))
            .GroupBy(x => x.GiaiDoanId)
            .ToDictionary(g => g.Key, g => g.Min(x => x.i));

        var groups = items
            .GroupBy(h => h.GiaiDoanId)
            .Select(g =>
            {
                if (g.Key is int id && giaiDoanTenById.TryGetValue(id, out var ten))
                {
                    return new KeHoachTrienKhaiGroupByGiaiDoanDto
                    {
                        GiaiDoanId = id,
                        TenGiaiDoan = ten,
                        SortOrder = giaiDoanSortById.GetValueOrDefault(id, int.MaxValue - 1),
                        Items = g.OrderBy(x => itemIndexById.GetValueOrDefault(x.Id, int.MaxValue)).ToList(),
                    };
                }

                return new KeHoachTrienKhaiGroupByGiaiDoanDto
                {
                    GiaiDoanId = null,
                    TenGiaiDoan = KhacGiaiDoanTen,
                    SortOrder = int.MaxValue,
                    Items = g.OrderBy(x => itemIndexById.GetValueOrDefault(x.Id, int.MaxValue)).ToList(),
                };
            })
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => firstGroupIndexByGiaiDoanId.GetValueOrDefault(g.GiaiDoanId, int.MaxValue))
            .ToList();

        var rows = new List<KeHoachTrienKhaiHangMucExportItemDto>();
        for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var group = groups[groupIndex];
            rows.Add(new KeHoachTrienKhaiHangMucExportItemDto
            {
                Stt = ToGroupLetter(groupIndex),
                GiaiDoan = group.TenGiaiDoan,
                IsGroupHeader = true,
            });

            var itemStt = 1;
            foreach (var hangMuc in group.Items)
                rows.Add(MapItem(hangMuc, itemStt++, donViTenById, userTenById));
        }

        return rows;
    }

    private static KeHoachTrienKhaiHangMucExportItemDto MapItem(
        HangMucKeHoach hangMuc,
        int stt,
        IReadOnlyDictionary<long, string> donViTenById,
        IReadOnlyDictionary<long, string> userTenById) =>
        new()
        {
            Stt = stt.ToString(),
            TenHangMuc = hangMuc.TenHangMuc,
            DonViChuTri = ResolveName(hangMuc.DonViChuTriId, donViTenById),
            DonViPhoiHop = JoinNames(hangMuc.DonViPhoiHopIds, donViTenById),
            NgayBatDau = hangMuc.NgayBatDau,
            NgayKetThuc = hangMuc.NgayKetThuc,
            ThoiHan = CalcThoiHan(hangMuc),
            CanBoChuTri = ResolveName(hangMuc.CanBoChuTriId, userTenById),
            CanBoPhoiHop = JoinNames(hangMuc.CanBoPhoiHopIds, userTenById),
            KinhPhi = hangMuc.KinhPhi,
        };

    private static string ToGroupLetter(int index) =>
        ((char)('A' + index)).ToString();

    private static int? CalcThoiHan(HangMucKeHoach hangMuc)
    {
        if (hangMuc.NgayBatDau.HasValue && hangMuc.NgayKetThuc.HasValue)
            return hangMuc.NgayKetThuc.Value.DayNumber - hangMuc.NgayBatDau.Value.DayNumber + 1;

        return null;
    }

    private static string ResolveName(long? id, IReadOnlyDictionary<long, string> lookup) =>
        id is long key && lookup.TryGetValue(key, out var name) ? name : string.Empty;

    private static string JoinNames(IEnumerable<long>? ids, IReadOnlyDictionary<long, string> lookup)
    {
        if (ids == null)
            return string.Empty;

        return string.Join(", ",
            ids.Select(id => lookup.GetValueOrDefault(id, string.Empty))
                .Where(name => !string.IsNullOrWhiteSpace(name)));
    }
}
