using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Constants;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuAnBuocs.DTOs;

namespace QLDA.Application.DuAnBuocs.Queries;

public record DuAnBuocGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<DuAnBuocDto>> {
    public int? QuyTrinhId { get; set; }
    public Guid? DuAnId { get; set; }
    public string? GlobalFilter { get; set; }
}

public record DuAnBuocGetDanhSachQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DuAnBuocGetDanhSachQuery, PaginatedList<DuAnBuocDto>> {
    private readonly IRepository<DuAnBuoc, int> DuAnBuoc =
        ServiceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth =
        ServiceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext =
        ServiceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<PaginatedList<DuAnBuocDto>> Handle(DuAnBuocGetDanhSachQuery request,
        CancellationToken cancellationToken) {
        var baseSet = DuAnBuoc.GetQueryableSet(OnlyUsed: false);
        var query = _buocAuth.FilterVisibleSteps(baseSet, _authContext)
                .AsNoTracking()
                .Where(e => !e.DuAn!.IsDeleted)
                .WhereIf(request.QuyTrinhId > 0, e => e.Buoc!.QuyTrinhId == request.QuyTrinhId)
                .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
                .WhereGlobalFilter(
                    request,
                    e => e.TenBuoc
                )
            ;

        var entities = await query
            .OrderBy(e => e.Buoc!.Level).ThenBy(e => e.Buoc!.Stt)
            .Include(e => e.DuAnBuocManHinhs!)
                .ThenInclude(m => m.ManHinh)
            .Include(e => e.Buoc!)
                .ThenInclude(b => b.BuocManHinhs!)
                .ThenInclude(m => m.ManHinh)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(entity => {
            var manHinhs = entity.DuAnBuocManHinhs is { Count: > 0 }
                ? entity.DuAnBuocManHinhs.OrderByDefault().Select(i => i.RightId).ToList()
                : entity.Buoc?.BuocManHinhs?.OrderByDefault().Select(i => i.RightId).ToList() ?? [];
            var pv = entity.PartialView ?? entity.Buoc?.PartialView;

            return new DuAnBuocDto() {
                Id = entity.Id,
                DuAnId = entity.DuAnId,
                ParentId = entity.Buoc!.ParentId,
                GiaiDoanId = entity.Buoc.GiaiDoanId,
                Level = entity.Buoc.Level,
                Path = entity.Buoc.Path,
                Stt = entity.Buoc.Stt ?? 0,
                QuyTrinhId = entity.Buoc.QuyTrinhId,
                BuocId = entity.BuocId,
                TenBuoc = entity.TenBuoc ?? entity.Buoc.Ten,
                PartialView = entity.PartialView,
                DanhSachManHinh = manHinhs
            };
        });

        return PaginatedList<DuAnBuocDto>.Create(dtos, request.Skip(), request.Take());
    }
}
