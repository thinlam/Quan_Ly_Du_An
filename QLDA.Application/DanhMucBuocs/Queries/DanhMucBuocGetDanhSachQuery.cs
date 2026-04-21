using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DanhMucBuocs.DTOs;

namespace QLDA.Application.DanhMucBuocs.Queries;

public record DanhMucBuocGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<DanhMucBuocDto>> {
    public int? QuyTrinhId { get; set; }
    public int? GiaiDoanId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsCbo { get; set; }
    public List<long>? Ids { get; set; }
}

public record DanhMucBuocGetDanhSachQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DanhMucBuocGetDanhSachQuery, PaginatedList<DanhMucBuocDto>> {
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc =
        ServiceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();

    public async Task<PaginatedList<DanhMucBuocDto>> Handle(DanhMucBuocGetDanhSachQuery request,
        CancellationToken cancellationToken) {
        var query = DanhMucBuoc.GetQueryableSet(
                OnlyUsed: false
            )
            .AsNoTracking()
            .WhereIf(request.QuyTrinhId > 0, e => e.QuyTrinhId == request.QuyTrinhId)
            .WhereIf(request.GiaiDoanId > 0, e => e.GiaiDoanId == request.GiaiDoanId)

            .WhereFunc(request.IsCbo,
                q => q
                    .WhereIf(request.Ids != null, e => request.Ids!.Contains(e.Id) || e.Used, e => e.Used)
            )
            .WhereGlobalFilter(
                request,
                    e => e.Ten
            )
        ;


        return await query
            .OrderByDescending(e => e.QuyTrinhId)
            .ThenBy(e => e.Level)
            .ThenBy(e => e.Stt)
            .Select(entity => new DanhMucBuocDto() {
                Id = entity.Id,
                ParentId = entity.ParentId,
                GiaiDoanId = entity.GiaiDoanId,
                Level = entity.Level,
                Ma = entity.Ma,
                MoTa = entity.MoTa,
                Path = entity.Path,
                QuyTrinhId = entity.QuyTrinhId,
                Stt = entity.Stt,
                Used = entity.Used,
                Ten = entity.Ten,
                DanhSachManHinh = entity.BuocManHinhs!
                    .OrderBy(i => i.Stt)
                    .Select(i => i.ManHinhId).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}