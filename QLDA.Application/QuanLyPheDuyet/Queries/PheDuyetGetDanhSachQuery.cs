using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Queries;

/// <summary>
/// Danh sach tong hop tat ca ban ghi pheduyet voi trang thai moi nhat tu PheDuyetHistory
/// </summary>
public record PheDuyetGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PheDuyetListItemDto>> {
    public Guid? DuAnId { get; set; }
    public string? Type { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class PheDuyetGetDanhSachQueryHandler : IRequestHandler<PheDuyetGetDanhSachQuery, PaginatedList<PheDuyetListItemDto>> {
    private readonly IRepository<PheDuyetDuToan, Guid> _duToanRepo;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepo;

    public PheDuyetGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _duToanRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _historyRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
    }

    public async Task<PaginatedList<PheDuyetListItemDto>> Handle(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken) {
        if (request.Type != null && request.Type != PheDuyetEntityNames.PheDuyetDuToan) {
            // Future: them entity type khac tai day
            return new PaginatedList<PheDuyetListItemDto>([], 0, request.Skip(), request.Take());
        }

        return await GetDuToanItems(request, cancellationToken);
    }

    private async Task<PaginatedList<PheDuyetListItemDto>> GetDuToanItems(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken) {
        // Subquery: latest history per entity
        var latestHistory = _historyRepo.GetQueryableSet()
            .Where(h => h.EntityName == PheDuyetEntityNames.PheDuyetDuToan)
            .GroupBy(h => h.EntityId)
            .Select(g => new { EntityId = g.Key, NgayXuLy = g.Max(x => x.NgayXuLy) });

        // Main query: join entity + latest history
        var query = _duToanRepo.GetQueryableSet().AsNoTracking()
            .Include(e => e.TrangThai)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereGlobalFilter(
                request,
                e => e.So,
                e => e.NguoiKy,
                e => e.TrichYeu
            )
            .Select(e => new PheDuyetListItemDto {
                Id = e.Id,
                Type = PheDuyetEntityNames.PheDuyetDuToan,
                DuAnId = e.DuAnId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,
                SoVanBan = e.So,
                TrichYeu = e.TrichYeu,
                NguoiKy = e.NguoiKy,
                NgayKy = e.NgayKy,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null ? e.TrangThai.Ma : null,
                TenTrangThai = e.TrangThai != null ? e.TrangThai.Ten : null,
                NgayXuLyMoiNhat = latestHistory
                    .Where(h => h.EntityId == e.Id)
                    .Select(h => (DateTimeOffset?)h.NgayXuLy)
                    .FirstOrDefault()
            })
            .OrderByDescending(i => i.NgayXuLyMoiNhat ?? DateTimeOffset.MinValue);

        return await query.PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
