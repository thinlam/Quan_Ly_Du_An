using BuildingBlocks.CrossCutting.DateTimes;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuAns.Queries;
using QLDA.Application.KySos.DTOs;


namespace QLDA.Application.KySos.Queries;

internal sealed class NoiDungDaKyJoinedRow {
    public Attachment E { get; init; } = null!;
    public UserMaster? User { get; init; }
}

internal static class NoiDungDaKyQueryableExtensions {
    internal static (DateOnly? TuNgay, DateOnly? DenNgay) ResolveDateRange(
        NoiDungDaKySearchDto search,
        IDateTimeProvider clock) {
        var today = DateOnly.FromDateTime(clock.OffsetNow.LocalDateTime);
        var tuNgay = search.TuNgay;
        var denNgay = search.DenNgay;

        if (!tuNgay.HasValue && !denNgay.HasValue)
            return (today.AddYears(-1), today);

        if (tuNgay.HasValue && !denNgay.HasValue)
            return (tuNgay, today);

        return (tuNgay, denNgay);
    }

    internal static async Task<List<NoiDungDaKyJoinedRow>> ApplyFiltersAsync(
        this IQueryable<Attachment> query,
        NoiDungDaKySearchDto search,
        IQueryable<UserMaster> users,
        IServiceProvider serviceProvider,
        IDateTimeProvider clock,
        CancellationToken cancellationToken) {
        var (tuNgay, denNgay) = ResolveDateRange(search, clock);
        List<string>? groupIds = null;

        if (search.DuAnId.HasValue) {
            groupIds = await DuAnTepDinhKemGroupIdQueryExtensions.ResolveGroupIdsAsync(
                search.DuAnId.Value, serviceProvider, cancellationToken);
        }

        var nguoiKyId = search.NguoiKyId?.ToString();
        var filterLower = search.GlobalFilter?.Trim().ToLower(
            System.Globalization.CultureInfo.CurrentCulture);

        var files = await query
            .Where(e => e.ParentId != null)
            .Where(e => e.GroupType.Contains("KySo"))
            .WhereIf(nguoiKyId != null, e => e.CreatedBy == nguoiKyId)
            .WhereIf(groupIds != null, e => groupIds!.Contains(e.GroupId))
            .ToListAsync(cancellationToken);

        if (tuNgay.HasValue)
            files = files.Where(e => e.CreatedAt >= tuNgay.Value.ToStartOfDayUtc()).ToList();
        if (denNgay.HasValue)
            files = files.Where(e => e.CreatedAt <= denNgay.Value.ToEndOfDayUtc()).ToList();

        if (files.Count == 0)
            return [];

        var createdByIds = files
            .Select(e => e.CreatedBy)
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

        var userMap = createdByIds.Count == 0
            ? new Dictionary<string, UserMaster>()
            : (await users
                .Where(u => u.UserPortalId.HasValue
                         && createdByIds.Contains(u.UserPortalId.Value.ToString()))
                .ToListAsync(cancellationToken))
                .GroupBy(u => u.UserPortalId!.Value.ToString())
                .ToDictionary(g => g.Key, g => g.First());

        IEnumerable<NoiDungDaKyJoinedRow> rows = files
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new NoiDungDaKyJoinedRow {
                E = e,
                User = userMap.GetValueOrDefault(e.CreatedBy),
            });

        if (!string.IsNullOrWhiteSpace(filterLower)) {
            rows = rows.Where(x =>
                (x.E.FileName != null && x.E.FileName.Contains(filterLower, StringComparison.OrdinalIgnoreCase))
                || (x.E.OriginalName != null && x.E.OriginalName.Contains(filterLower, StringComparison.OrdinalIgnoreCase))
                || (x.User?.HoTen != null && x.User.HoTen.Contains(filterLower, StringComparison.OrdinalIgnoreCase)));
        }

        return rows.ToList();
    }

    internal static string FormatDungLuong(long sizeBytes) =>
        sizeBytes switch {
            < 1024 => $"{sizeBytes} B",
            < 1024 * 1024 => $"{sizeBytes / 1024.0:0.#} KB",
            _ => $"{sizeBytes / (1024.0 * 1024.0):0.##} MB"
        };
}
