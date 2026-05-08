using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.QuanLyPheDuyet.DTOs;

namespace QLDA.Application.QuanLyPheDuyet.Queries;

/// <summary>
/// Lich su PheDuyetHistory, sort moi nhat truoc
/// </summary>
public record PheDuyetGetLichSuQuery : AggregateRootPagination, IRequest<PaginatedList<PheDuyetHistoryDto>> {
    public Guid? DuAnId { get; set; }
    public string? Type { get; set; }
    public Guid? EntityId { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class PheDuyetGetLichSuQueryHandler : IRequestHandler<PheDuyetGetLichSuQuery, PaginatedList<PheDuyetHistoryDto>> {
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepo;

    public PheDuyetGetLichSuQueryHandler(IServiceProvider serviceProvider) {
        _historyRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
    }

    public async Task<PaginatedList<PheDuyetHistoryDto>> Handle(PheDuyetGetLichSuQuery request, CancellationToken cancellationToken) {
        var query = _historyRepo.GetQueryableSet()
            .Include(h => h.TrangThai)
            .Include(h => h.DuAn)
            .WhereIf(request.DuAnId != null, h => h.DuAnId == request.DuAnId)
            .WhereIf(!string.IsNullOrEmpty(request.Type), h => h.EntityName == request.Type)
            .WhereIf(request.EntityId != null, h => h.EntityId == request.EntityId)
            .Select(h => new PheDuyetHistoryDto {
                Id = h.Id,
                EntityName = h.EntityName,
                EntityId = h.EntityId,
                DuAnId = h.DuAnId,
                NguoiXuLyId = h.NguoiXuLyId,
                TrangThaiId = h.TrangThaiId,
                MaTrangThai = h.TrangThai != null ? h.TrangThai.Ma : null,
                TenTrangThai = h.TrangThai != null ? h.TrangThai.Ten : null,
                NoiDung = h.NoiDung,
                NgayXuLy = h.NgayXuLy,
                TenDuAn = h.DuAn != null ? h.DuAn.TenDuAn : null
            });

        // Sort client-side (SQLite can't OrderBy DateTimeOffset)
        var items = (await query.ToListAsync(cancellationToken))
            .OrderByDescending(h => h.NgayXuLy)
            .ToList();

        return new PaginatedList<PheDuyetHistoryDto>(items.Skip(request.Skip()).Take(request.Take()).ToList(), items.Count, request.Skip(), request.Take());
    }
}
