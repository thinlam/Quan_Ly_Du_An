using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;

namespace QLDA.Application.DmHinhThucLuaChonNhaThaus.Queries;

public record DmHinhThucLuaChonNhaThauGetDanhSachQuery : AggregateRootPagination, IRequest<PaginatedList<DanhMucHinhThucLuaChonNhaThau>>
{

    public bool GetAll { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}
public record DanhMucNguonVonGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DmHinhThucLuaChonNhaThauGetDanhSachQuery, PaginatedList<DanhMucHinhThucLuaChonNhaThau>> {

    private readonly IRepository<DanhMucHinhThucLuaChonNhaThau, int> DmHinhThucLuaChonNhaThau =
        serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucLuaChonNhaThau, int>>();



    public async Task<PaginatedList<DanhMucHinhThucLuaChonNhaThau>> Handle(DmHinhThucLuaChonNhaThauGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var query = DmHinhThucLuaChonNhaThau.GetQueryableSet().AsNoTracking()
           .WhereIf( request.GetAll, e => request.GetAll )
           .WhereIf( !request.GetAll, e =>  e.Used)
           .WhereFunc(request.IsNoTracking, e => e.AsNoTracking());


        return await query
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);


    }
}