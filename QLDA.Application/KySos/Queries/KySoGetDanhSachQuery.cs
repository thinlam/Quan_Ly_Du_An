using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.KySos.DTOs;

namespace QLDA.Application.KySos.Queries;

public record KySoGetDanhSachQuery(KySoSearchDto SearchDto) 
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<KySoDto>> {
    public string? GlobalFilter { get; set; }
}

internal class KySoGetDanhSachQueryHandler : IRequestHandler<KySoGetDanhSachQuery, PaginatedList<KySoDto>> {
    private readonly IRepository<KySo, Guid> KySo;

    public KySoGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        KySo = serviceProvider.GetRequiredService<IRepository<KySo, Guid>>();
    }

    public async Task<PaginatedList<KySoDto>> Handle(KySoGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = KySo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Include(e => e.PhuongThucKySo)
            .Include(e => e.ChucVu)
            .WhereGlobalFilter(
                request,
                e => e.Email,
                e => e.SerialChungThu,
                e => e.ToChucCap
            );

        return await queryable
            .Select(e => e.ToDto())
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}