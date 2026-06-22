using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;

namespace QLDA.Application.DuAns.Queries;

public record DuAnGetDanhSachComboboxQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<object> {
    public Guid? DuAnId { get; set; }
    public string? GlobalFilter { get; set; }
}

internal class
    DuAnGetDanhSachComboboxQueryHandler : IRequestHandler<DuAnGetDanhSachComboboxQuery, object> {
    private readonly IRepository<DuAn, Guid> DuAn;

    public DuAnGetDanhSachComboboxQueryHandler(IServiceProvider serviceProvider) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    }


    public async Task<object> Handle(DuAnGetDanhSachComboboxQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = DuAn.GetQueryableSet().AsNoTracking()
            .WhereFunc(request.DuAnId != null, q => q.OrderByDescending(e => e.Id == request.DuAnId))
            .WhereGlobalFilter(
                request,
                e => e.TenDuAn,
                e => e.MaDuAn
            );

        return await queryable
            .Select(e => new {
                e.Id,
                e.TenDuAn,
                e.ParentId,
                e.Level,
                e.Path
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}