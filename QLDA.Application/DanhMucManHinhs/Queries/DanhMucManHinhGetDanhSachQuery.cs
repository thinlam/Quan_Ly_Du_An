using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Constants;
using QLDA.Application.DanhMucManHinhs.DTOs;

namespace QLDA.Application.DanhMucManHinhs.Queries;

public record DanhMucManHinhGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<DanhMucManHinhDto>> {
    public string? GlobalFilter { get; set; }
    public bool IsCbo { get; set; }
    public List<int>? Ids { get; set; }

}

public record DanhMucManHinhGetDanhSachQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DanhMucManHinhGetDanhSachQuery, PaginatedList<DanhMucManHinhDto>> {
    private readonly IRepository<DanhMucManHinh, int> DanhMucManHinh =
        ServiceProvider.GetRequiredService<IRepository<DanhMucManHinh, int>>();

    public async Task<PaginatedList<DanhMucManHinhDto>> Handle(DanhMucManHinhGetDanhSachQuery request,
        CancellationToken cancellationToken) {
        var query = DanhMucManHinh.GetOrderedSet()
            .WhereFunc(request.IsCbo,
                q => q //Combobox
                       //Còn sử dụng (used = true) và thuộc ids
                    .WhereIf(request.Ids != null, e => request.Ids!.Contains(e.Id) || e.Used, e => e.Used)
            )
            .WhereGlobalFilter(
                request,
                e => e.Ten,
                e => e.Label,
                e => e.Title
            );

        var allItems = await query.ToListAsync(cancellationToken);
        var ordered = allItems.OrderByDefault();

        var dtos = ordered.Select(entity => new DanhMucManHinhDto() {
            Id = entity.Id,
            Ten = entity.Ten ?? string.Empty,
            Label = entity.Label,
            Title = entity.Title,
            Used = entity.Used,
        });

        return PaginatedList<DanhMucManHinhDto>.Create(dtos, request.Skip(), request.Take());
    }
}