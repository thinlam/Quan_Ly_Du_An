using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;

namespace QLDA.Application.QuyetDinhDieuChinhs.Queries;

public record QuyetDinhDieuChinhGetDanhSachQuery(
    Guid? DuAnId = null,
    string? PheDuyetEntityName = null,
    Guid? PheDuyetEntityId = null,
    string? GlobalFilter = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResultDto<QuyetDinhDieuChinhListItemDto>>;

public record QuyetDinhDieuChinhListItemDto {
    public Guid Id { get; set; }
    public string PheDuyetEntityName { get; set; } = default!;
    public Guid PheDuyetEntityId { get; set; }
    public Guid DuAnId { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public int LoaiDieuChinhId { get; set; }
    public string? TenLoaiDieuChinh { get; set; }
    public int TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public int Lan { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public record PagedResultDto<T>(List<T> Items, int TotalCount, int Page, int PageSize);

internal class QuyetDinhDieuChinhGetDanhSachQueryHandler : IRequestHandler<QuyetDinhDieuChinhGetDanhSachQuery, PagedResultDto<QuyetDinhDieuChinhListItemDto>> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;

    public QuyetDinhDieuChinhGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
    }

    public async Task<PagedResultDto<QuyetDinhDieuChinhListItemDto>> Handle(QuyetDinhDieuChinhGetDanhSachQuery request, CancellationToken cancellationToken) {
        var query = _repository.GetQueryableSet()
            .Include(e => e.LoaiDieuChinh)
            .Include(e => e.TrangThai)
            .Where(e => !e.IsDeleted);

        if (request.DuAnId.HasValue) {
            query = query.Where(e => e.DuAnId == request.DuAnId.Value);
        }

        if (!string.IsNullOrEmpty(request.PheDuyetEntityName)) {
            query = query.Where(e => e.PheDuyetEntityName == request.PheDuyetEntityName);
        }

        if (request.PheDuyetEntityId.HasValue) {
            query = query.Where(e => e.PheDuyetEntityId == request.PheDuyetEntityId.Value);
        }

        if (!string.IsNullOrEmpty(request.GlobalFilter)) {
            var filter = request.GlobalFilter.ToLower();
            query = query.Where(e => (e.SoQuyetDinh != null && e.SoQuyetDinh.Contains(filter))
                      || (e.TrichYeu != null && e.TrichYeu.Contains(filter))
                      || (e.LyDo != null && e.LyDo.Contains(filter)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new QuyetDinhDieuChinhListItemDto {
                Id = e.Id,
                PheDuyetEntityName = e.PheDuyetEntityName,
                PheDuyetEntityId = e.PheDuyetEntityId,
                DuAnId = e.DuAnId,
                SoQuyetDinh = e.SoQuyetDinh,
                NgayQuyetDinh = e.NgayQuyetDinh,
                TrichYeu = e.TrichYeu,
                LoaiDieuChinhId = e.LoaiDieuChinhId,
                TenLoaiDieuChinh = e.LoaiDieuChinh != null ? e.LoaiDieuChinh.Ten : null,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null ? e.TrangThai.Ma : null,
                TenTrangThai = e.TrangThai != null ? e.TrangThai.Ten : null,
                Lan = e.Lan,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<QuyetDinhDieuChinhListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
}