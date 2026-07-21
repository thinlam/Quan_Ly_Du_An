using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application.Common.DTOs;

public class PaginatedList<T> {
    /// <summary>
    /// Thống kê tổng hợp
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Statistic { get; set; } = null;
    /// <summary>
    /// Tổng số dòng
    /// </summary>
    public int TotalRows { get; }
    /// <summary>
    /// Vị trí trang hiện tại
    /// </summary>
    public int PageNumber { get; }
    /// <summary>
    /// Tổng số trang
    /// </summary>
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public List<T> Data { get; set; } = [];
    public PaginatedList() {
    }

    public PaginatedList(IReadOnlyCollection<T> items, int count, int skip, int pageSize) {
        PageNumber = pageSize > 0 ? skip / pageSize + 1 : 1;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 1;
        TotalRows = count;
        Data = [.. items];
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int skip, int take,
        CancellationToken cancellationToken = default) {
        int count = await source.CountAsync(cancellationToken: cancellationToken);
        if (skip >= 0 && take > 0)
            source = source.Skip(skip).Take(take);

        return new PaginatedList<T>(await source.ToListAsync(cancellationToken: cancellationToken), count, skip, take);
    }
    public static PaginatedList<T> Create(IEnumerable<T> source, int skip, int take) {
        int count = source.Count();
        if (skip >= 0 && take > 0)
            source = source.Skip(skip).Take(take);

        return new PaginatedList<T>([.. source], count, skip, take);
    }
}
