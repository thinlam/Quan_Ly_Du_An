using BuildingBlocks.CrossCutting.Offices;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs;

internal static class KeHoachTrienKhaiHangMucImportGiaiDoanHelper {
    public const string GiaiDoanDuAnSeparator = " - ";

    public static string FormatDisplayName(string tenGiaiDoan, string tenDuAn) =>
        $"{tenGiaiDoan.Trim()}{GiaiDoanDuAnSeparator}{tenDuAn.Trim()}";

    public static bool TryParseDisplayName(
        string raw,
        out string tenGiaiDoan,
        out string tenDuAnFromLabel) {
        tenGiaiDoan = raw.Trim();
        tenDuAnFromLabel = string.Empty;

        var idx = raw.LastIndexOf(GiaiDoanDuAnSeparator, StringComparison.Ordinal);
        if (idx <= 0)
            return false;

        tenGiaiDoan = raw[..idx].Trim();
        tenDuAnFromLabel = raw[(idx + GiaiDoanDuAnSeparator.Length)..].Trim();
        return tenGiaiDoan.Length > 0 && tenDuAnFromLabel.Length > 0;
    }

    public static async Task<List<ComboData>> LoadGiaiDoanComboAsync(
        IRepository<DuAn, Guid> duAnRepo,
        IRepository<DuAnBuoc, int> duAnBuocRepo,
        IRepository<DanhMucGiaiDoan, int> giaiDoanRepo,
        IQueryable<DuAn> visibleDuAnQuery,
        Guid? duAnId,
        CancellationToken cancellationToken) {
        var duAnQuery = visibleDuAnQuery.AsNoTracking();
        if (duAnId.HasValue)
            duAnQuery = duAnQuery.Where(e => e.Id == duAnId.Value);

        var duAnRows = await duAnQuery
            .Where(e => e.TenDuAn != null && e.TenDuAn != "")
            .Select(e => new { e.Id, e.TenDuAn })
            .ToListAsync(cancellationToken);

        if (duAnRows.Count == 0)
            return [];

        var duAnIds = duAnRows.Select(e => e.Id).ToList();
        var phasesByDuAn = await LoadRootPhasesByDuAnAsync(
            duAnBuocRepo,
            giaiDoanRepo,
            duAnIds,
            cancellationToken);

        var combos = new List<ComboData>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var duAn in duAnRows.OrderBy(e => e.TenDuAn, StringComparer.OrdinalIgnoreCase)) {
            if (!phasesByDuAn.TryGetValue(duAn.Id, out var phases))
                continue;

            foreach (var phase in phases) {
                var name = duAnId.HasValue
                    ? phase.DisplayName
                    : FormatDisplayName(phase.DisplayName, duAn.TenDuAn!);

                if (!seenNames.Add(name))
                    continue;

                combos.Add(new ComboData {
                    Id = phase.GiaiDoanId.ToString(),
                    Name = name,
                });
            }
        }

        return combos;
    }

    public static async Task<Dictionary<Guid, Dictionary<string, int>>> LoadGiaiDoanLookupByDuAnAsync(
        IRepository<DuAn, Guid> duAnRepo,
        IRepository<DuAnBuoc, int> duAnBuocRepo,
        IRepository<DanhMucGiaiDoan, int> giaiDoanRepo,
        IEnumerable<Guid> duAnIds,
        CancellationToken cancellationToken) {
        var idList = duAnIds.Distinct().ToList();
        if (idList.Count == 0)
            return [];

        var phasesByDuAn = await LoadRootPhasesByDuAnAsync(
            duAnBuocRepo,
            giaiDoanRepo,
            idList,
            cancellationToken);

        var result = new Dictionary<Guid, Dictionary<string, int>>();

        foreach (var (projectId, phases) in phasesByDuAn) {
            var byTen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var phase in phases) {
                RegisterLookupName(byTen, phase.DisplayName, phase.GiaiDoanId);

                if (!string.IsNullOrWhiteSpace(phase.GiaiDoanTen)
                    && !string.Equals(phase.GiaiDoanTen, phase.DisplayName, StringComparison.OrdinalIgnoreCase)) {
                    RegisterLookupName(byTen, phase.GiaiDoanTen, phase.GiaiDoanId);
                }
            }

            result[projectId] = byTen;
        }

        return result;
    }

    public static bool TryResolveGiaiDoanId(
        string? rawTenGiaiDoan,
        string? tenDuAnOnRow,
        Guid duAnId,
        IReadOnlyDictionary<Guid, Dictionary<string, int>> lookupByDuAn,
        out int giaiDoanId,
        out string? error) {
        giaiDoanId = default;
        error = null;

        if (string.IsNullOrWhiteSpace(rawTenGiaiDoan)) {
            error = "Giai đoạn bắt buộc";
            return false;
        }

        var tenGiaiDoan = rawTenGiaiDoan.Trim();
        if (TryParseDisplayName(tenGiaiDoan, out var parsedGiaiDoan, out var parsedDuAn)) {
            if (!string.Equals(parsedDuAn, tenDuAnOnRow?.Trim(), StringComparison.OrdinalIgnoreCase)) {
                error = "Giai đoạn không khớp dự án trên dòng";
                return false;
            }

            tenGiaiDoan = parsedGiaiDoan;
        }

        if (!lookupByDuAn.TryGetValue(duAnId, out var giaiDoanByTen)
            || !giaiDoanByTen.TryGetValue(tenGiaiDoan, out giaiDoanId)) {
            error = "Không tìm thấy giai đoạn";
            return false;
        }

        return true;
    }

    /// <summary>
    /// GiaiDoanId → SortOrder theo quy trình dự án (DuAnBuoc.Buoc.Stt).
    /// Dùng chung import combo, Excel export, Word phiếu trình.
    /// </summary>
    internal static async Task<IReadOnlyDictionary<int, int>> GetGiaiDoanSortByDuAnAsync(
        Guid duAnId,
        IRepository<DuAnBuoc, int> duAnBuocRepo,
        IRepository<DanhMucGiaiDoan, int> giaiDoanRepo,
        CancellationToken cancellationToken = default)
    {
        var phasesByDuAn = await LoadRootPhasesByDuAnAsync(
            duAnBuocRepo,
            giaiDoanRepo,
            [duAnId],
            cancellationToken);

        if (!phasesByDuAn.TryGetValue(duAnId, out var phases) || phases.Count == 0)
            return new Dictionary<int, int>();

        return phases.ToDictionary(p => p.GiaiDoanId, p => p.SortOrder);
    }

    /// <summary>GiaiDoanId → tên hiển thị theo quy trình dự án (Buoc.Ten / TenBuoc).</summary>
    internal static async Task<IReadOnlyDictionary<int, string>> GetGiaiDoanDisplayNameByDuAnAsync(
        Guid duAnId,
        IRepository<DuAnBuoc, int> duAnBuocRepo,
        IRepository<DanhMucGiaiDoan, int> giaiDoanRepo,
        CancellationToken cancellationToken = default)
    {
        var phasesByDuAn = await LoadRootPhasesByDuAnAsync(
            duAnBuocRepo,
            giaiDoanRepo,
            [duAnId],
            cancellationToken);

        if (!phasesByDuAn.TryGetValue(duAnId, out var phases) || phases.Count == 0)
            return new Dictionary<int, string>();

        return phases.ToDictionary(p => p.GiaiDoanId, p => p.DisplayName);
    }

    private static async Task<Dictionary<Guid, List<ProjectPhaseLookup>>> LoadRootPhasesByDuAnAsync(
        IRepository<DuAnBuoc, int> duAnBuocRepo,
        IRepository<DanhMucGiaiDoan, int> giaiDoanRepo,
        IReadOnlyCollection<Guid> duAnIds,
        CancellationToken cancellationToken) {
        var phaseRows = await duAnBuocRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(dab => duAnIds.Contains(dab.DuAnId))
            .Where(dab => dab.Buoc != null && dab.Buoc.GiaiDoanId != null)
            .Where(dab => dab.Buoc!.ParentId == null || dab.Buoc.ParentId == 0)
            .Select(dab => new {
                dab.DuAnId,
                GiaiDoanId = dab.Buoc!.GiaiDoanId!.Value,
                DisplayName = dab.Buoc.Ten ?? dab.TenBuoc ?? string.Empty,
                SortOrder = dab.Buoc.Stt ?? int.MaxValue - 1,
            })
            .ToListAsync(cancellationToken);

        if (phaseRows.Count == 0)
            return [];

        var giaiDoanIds = phaseRows
            .Select(x => x.GiaiDoanId)
            .Distinct()
            .ToList();

        var giaiDoanTenById = await giaiDoanRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(g => giaiDoanIds.Contains(g.Id))
            .Select(g => new { g.Id, g.Ten })
            .ToDictionaryAsync(g => g.Id, g => g.Ten ?? string.Empty, cancellationToken);

        return phaseRows
            .Where(x => !string.IsNullOrWhiteSpace(x.DisplayName))
            .GroupBy(x => x.DuAnId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .GroupBy(x => x.GiaiDoanId)
                    .Select(items => items.OrderBy(x => x.SortOrder).ThenBy(x => x.DisplayName).First())
                    .Select(x => new ProjectPhaseLookup(
                        x.GiaiDoanId,
                        (x.DisplayName ?? string.Empty).Trim(),
                        (giaiDoanTenById.GetValueOrDefault(x.GiaiDoanId) ?? string.Empty).Trim(),
                        x.SortOrder))
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .ToList());
    }

    private static void RegisterLookupName(Dictionary<string, int> byTen, string name, int giaiDoanId) {
        var trimmed = name.Trim();
        if (trimmed.Length == 0)
            return;

        byTen.TryAdd(trimmed, giaiDoanId);
    }

    private sealed record ProjectPhaseLookup(
        int GiaiDoanId,
        string DisplayName,
        string GiaiDoanTen,
        int SortOrder);
}
