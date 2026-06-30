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
        IRepository<DanhMucBuoc, int> danhMucBuocRepo,
        IRepository<DanhMucGiaiDoan, int> giaiDoanRepo,
        IQueryable<DuAn> visibleDuAnQuery,
        Guid? duAnId,
        CancellationToken cancellationToken) {
        var duAnQuery = visibleDuAnQuery.AsNoTracking();
        if (duAnId.HasValue)
            duAnQuery = duAnQuery.Where(e => e.Id == duAnId.Value);

        var duAnRows = await duAnQuery
            .Where(e => e.TenDuAn != null && e.TenDuAn != "")
            .Select(e => new { e.Id, e.TenDuAn, e.QuyTrinhId })
            .ToListAsync(cancellationToken);

        if (duAnRows.Count == 0)
            return [];

        var quyTrinhIds = duAnRows.Select(e => e.QuyTrinhId).Distinct().ToList();

        var giaiDoanTheoQuyTrinh = await danhMucBuocRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(b => quyTrinhIds.Contains(b.QuyTrinhId) && b.GiaiDoanId != null)
            .Select(b => new { b.QuyTrinhId, b.GiaiDoanId })
            .Distinct()
            .ToListAsync(cancellationToken);

        var giaiDoanIds = giaiDoanTheoQuyTrinh
            .Where(x => x.GiaiDoanId.HasValue)
            .Select(x => x.GiaiDoanId!.Value)
            .Distinct()
            .ToList();

        var giaiDoanTenById = await giaiDoanRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(g => giaiDoanIds.Contains(g.Id) && g.Ten != null && g.Ten != "")
            .Select(g => new { g.Id, g.Ten })
            .ToDictionaryAsync(g => g.Id, g => g.Ten!, cancellationToken);

        var combos = new List<ComboData>();

        foreach (var duAn in duAnRows.OrderBy(e => e.TenDuAn, StringComparer.OrdinalIgnoreCase)) {
            var phaseIds = giaiDoanTheoQuyTrinh
                .Where(x => x.QuyTrinhId == duAn.QuyTrinhId && x.GiaiDoanId.HasValue)
                .Select(x => x.GiaiDoanId!.Value)
                .Distinct();

            foreach (var phaseId in phaseIds.OrderBy(id => id)) {
                if (!giaiDoanTenById.TryGetValue(phaseId, out var tenGiaiDoan))
                    continue;

                combos.Add(new ComboData {
                    Id = phaseId.ToString(),
                    Name = duAnId.HasValue
                        ? tenGiaiDoan
                        : FormatDisplayName(tenGiaiDoan, duAn.TenDuAn!),
                });
            }
        }

        return combos
            .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static async Task<Dictionary<Guid, Dictionary<string, int>>> LoadGiaiDoanLookupByDuAnAsync(
        IRepository<DuAn, Guid> duAnRepo,
        IRepository<DanhMucBuoc, int> danhMucBuocRepo,
        IRepository<DanhMucGiaiDoan, int> giaiDoanRepo,
        IEnumerable<Guid> duAnIds,
        CancellationToken cancellationToken) {
        var idList = duAnIds.Distinct().ToList();
        if (idList.Count == 0)
            return [];

        var duAnRows = await duAnRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => idList.Contains(e.Id))
            .Select(e => new { e.Id, e.QuyTrinhId })
            .ToListAsync(cancellationToken);

        var quyTrinhIds = duAnRows.Select(e => e.QuyTrinhId).Distinct().ToList();

        var giaiDoanTheoQuyTrinh = await danhMucBuocRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(b => quyTrinhIds.Contains(b.QuyTrinhId) && b.GiaiDoanId != null)
            .Select(b => new { b.QuyTrinhId, b.GiaiDoanId })
            .Distinct()
            .ToListAsync(cancellationToken);

        var giaiDoanIds = giaiDoanTheoQuyTrinh
            .Where(x => x.GiaiDoanId.HasValue)
            .Select(x => x.GiaiDoanId!.Value)
            .Distinct()
            .ToList();

        var giaiDoanTenById = await giaiDoanRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(g => giaiDoanIds.Contains(g.Id) && g.Ten != null && g.Ten != "")
            .Select(g => new { g.Id, g.Ten })
            .ToDictionaryAsync(g => g.Id, g => g.Ten!, cancellationToken);

        var result = new Dictionary<Guid, Dictionary<string, int>>();

        foreach (var duAn in duAnRows) {
            var byTen = giaiDoanTheoQuyTrinh
                .Where(x => x.QuyTrinhId == duAn.QuyTrinhId && x.GiaiDoanId.HasValue)
                .Select(x => x.GiaiDoanId!.Value)
                .Distinct()
                .Where(giaiDoanTenById.ContainsKey)
                .GroupBy(
                    id => giaiDoanTenById[id].Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            result[duAn.Id] = byTen;
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
}
