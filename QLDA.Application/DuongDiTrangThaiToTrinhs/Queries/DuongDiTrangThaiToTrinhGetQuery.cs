using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Constants;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuongDiTrangThaiToTrinhs.DTOs;

namespace QLDA.Application.DuongDiTrangThaiToTrinhs.Queries;

public record DuongDiTrangThaiToTrinhGetQuery : AggregateRootPagination, IRequest<PaginatedList<DuongDiTrangThaiToTrinhDto>> {
    public string Loai { get; set; } = string.Empty;
    public string? MaTrangThaiHienTai { get; set; }

}

public record DuongDiTrangThaiToTrinhGetQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DuongDiTrangThaiToTrinhGetQuery, PaginatedList<DuongDiTrangThaiToTrinhDto>> {
    private readonly IRepository<DuongDiTrangThaiToTrinh, long> DuongDiTrangThaiToTrinh =
        ServiceProvider.GetRequiredService<IRepository<DuongDiTrangThaiToTrinh, long>>();
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> DanhMucTrangThaiPheDuyet =
      ServiceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
    public async Task<PaginatedList<DuongDiTrangThaiToTrinhDto>> Handle(DuongDiTrangThaiToTrinhGetQuery request, CancellationToken cancellationToken) {
        var query = DuongDiTrangThaiToTrinh.GetOrderedSet()
           .Where(e => e.Used && !(e.IsDeleted??false))
                    .WhereIf(request.Loai != null, e => request.Loai == e.Loai || e.Used, e => e.Used)
                    .WhereIf(request.MaTrangThaiHienTai != null, e => request.MaTrangThaiHienTai == e.MaTrangThaiHienTai) ;

        var allItems = await query.ToListAsync(cancellationToken);
        var dmTrangThaiPheDuyet = await DanhMucTrangThaiPheDuyet.GetQueryableSet().AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionaryAsync(x => x.Ma!, x => x.Ten ?? string.Empty, cancellationToken);

        var dtos = allItems.Select(entity => new DuongDiTrangThaiToTrinhDto
        {
            RoleId = entity.RoleId,
            RoleLevel = entity.RoleLevel,
            MaTrangThaiTiepTheo = entity.MaTrangThaiTiepTheo,
            TenTrangThaiTiepTheo = dmTrangThaiPheDuyet.TryGetValue(entity.MaTrangThaiTiepTheo,  out var tenTrangThai) 
                                    ? tenTrangThai : string.Empty
        });

       
        return PaginatedList<DuongDiTrangThaiToTrinhDto>.Create(dtos, request.Skip(), request.Take());
    }
}