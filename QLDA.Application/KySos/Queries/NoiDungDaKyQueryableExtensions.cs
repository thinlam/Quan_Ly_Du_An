using BuildingBlocks.CrossCutting.DateTimes;
using BuildingBlocks.CrossCutting.ExtensionMethods;
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuAns.Services;
using QLDA.Application.KySos.DTOs;
using QLDA.Domain.Constants;
using TepDinhKem = QLDA.Domain.Entities.TepDinhKem;

namespace QLDA.Application.KySos.Queries;

internal sealed class NoiDungDaKyJoinedRow
{
    public TepDinhKem E { get; init; } = null!;
    public UserMaster? User { get; init; }
}

internal static class NoiDungDaKyQueryableExtensions
{
    internal static (DateOnly? TuNgay, DateOnly? DenNgay) ResolveDateRange(
        NoiDungDaKySearchDto search,
        IDateTimeProvider clock)
    {
        if (!search.TuNgay.HasValue && !search.DenNgay.HasValue)
        {
            var today = DateOnly.FromDateTime(clock.OffsetNow.LocalDateTime);
            return (today.AddYears(-1), today);
        }

        return (search.TuNgay, search.DenNgay);
    }

    internal static async Task<List<NoiDungDaKyJoinedRow>> ApplyFiltersAsync(
        this IQueryable<TepDinhKem> query,
        NoiDungDaKySearchDto search,
        IQueryable<UserMaster> users,
        DuAnTepDinhKemGroupIdResolver duAnResolver,
        IDateTimeProvider clock,
        CancellationToken cancellationToken)
    {
        var (tuNgay, denNgay) = ResolveDateRange(search, clock);
        List<string>? groupIds = null;

        if (search.DuAnId.HasValue)
        {
            groupIds = await duAnResolver.ResolveGroupIdsAsync(
                search.DuAnId.Value, cancellationToken);
        }

        var nguoiKyId = search.NguoiKyId?.ToString();
        var filterLower = search.GlobalFilter?.Trim().ToLower(
            System.Globalization.CultureInfo.CurrentCulture);

        var files = await query
            .Where(e => e.ParentId != null)
            .Where(e => e.GroupType == GroupTypeConstants.KySo
                     || e.GroupType == GroupTypeConstants.NoiDungDaKySo)
            .WhereIf(nguoiKyId != null, e => e.CreatedBy == nguoiKyId)
            .WhereIf(groupIds != null, e => groupIds!.Contains(e.GroupId))
            .ToListAsync(cancellationToken);

        if (tuNgay.HasValue)
            files = files.Where(e => e.CreatedAt >= tuNgay.Value.ToStartOfDayUtc()).ToList();
        if (denNgay.HasValue)
            files = files.Where(e => e.CreatedAt <= denNgay.Value.ToEndOfDayUtc()).ToList();

        if (files.Count == 0)
            return [];

        var userList = await users.ToListAsync(cancellationToken);
        var userMap = userList
            .GroupBy(u => u.UserPortalId.ToString())
            .ToDictionary(g => g.Key, g => g.First());

        IEnumerable<NoiDungDaKyJoinedRow> rows = files
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new NoiDungDaKyJoinedRow
            {
                E = e,
                User = userMap.GetValueOrDefault(e.CreatedBy),
            });

        if (!string.IsNullOrWhiteSpace(filterLower))
        {
            rows = rows.Where(x =>
                (x.E.FileName != null && x.E.FileName.Contains(filterLower, StringComparison.OrdinalIgnoreCase))
                || (x.E.OriginalName != null && x.E.OriginalName.Contains(filterLower, StringComparison.OrdinalIgnoreCase))
                || (x.User?.HoTen != null && x.User.HoTen.Contains(filterLower, StringComparison.OrdinalIgnoreCase)));
        }

        return rows.ToList();
    }

    internal static string FormatDungLuong(long sizeBytes) =>
        sizeBytes switch
        {
            < 1024 => $"{sizeBytes} B",
            < 1024 * 1024 => $"{sizeBytes / 1024.0:0.#} KB",
            _ => $"{sizeBytes / (1024.0 * 1024.0):0.##} MB"
        };
}
