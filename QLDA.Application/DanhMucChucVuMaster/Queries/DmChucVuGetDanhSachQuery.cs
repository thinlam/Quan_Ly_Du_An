using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Domain.Entities;

namespace QLDA.Application.DmChucVus.Queries;

public record DmChucVuGetDanhSachQuery : AggregateRootPagination, IRequest<PaginatedList<DmChucVu>>
{
    public bool? GetAll { get; set; }
}

public record DmChucVuGetDanhSachQueryueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DmChucVuGetDanhSachQuery, PaginatedList<DmChucVu>>
{
    private readonly IRepository<DmChucVu, long> dmChucVu =
        ServiceProvider.GetRequiredService<IRepository<DmChucVu, long>>();

    public async Task<PaginatedList<DmChucVu>> Handle(DmChucVuGetDanhSachQuery request,
        CancellationToken cancellationToken)
    {
        try
        {

            var query = dmChucVu.GetQueryableSet().AsNoTracking()
                .Where(e => e.Used == true)
                .WhereIf(!request.GetAll ?? false, e => e.Used);

            return await query
                .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
        }
        catch (Exception ex)
        {

            throw;
        }

    }

}
