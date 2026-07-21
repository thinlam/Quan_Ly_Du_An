using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DanhMucNguonVons.DTOs;

namespace QLDA.Application.DanhMucNguonVons.Queries;

public record DanhMucNguonVonGetDanhSachQuery : AggregateRootPagination, IRequest<PaginatedList<DanhMucNguonVonDto>> {
    public bool GetAll { get; set; }
    public string? DuAnId { get; set; }
    public bool IsTracking { get; set; }
    public List<long>? Ids { get; set; }
}

public record DanhMucNguonVonGetDanhSachQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DanhMucNguonVonGetDanhSachQuery, PaginatedList<DanhMucNguonVonDto>> {
    private readonly IRepository<DanhMucNguonVon, int> DanhMucNguonVon =
        ServiceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();

    public async Task<PaginatedList<DanhMucNguonVonDto>> Handle(DanhMucNguonVonGetDanhSachQuery request,
        CancellationToken cancellationToken) {
        var query = DanhMucNguonVon.GetQueryableSet().AsNoTracking()
            .WhereIf(request.Ids != null, e => request.Ids!.Contains(e.Id) || e.Used || request.GetAll, e => request.GetAll || e.Used)

            .WhereIf(!string.IsNullOrWhiteSpace(request.DuAnId),
                e => e.DuAnNguonVons!.Any(i => i.LeftId.ToString() == request.DuAnId))
            .WhereFunc(request.IsTracking, e => e.AsNoTracking());


        return await query
            .Select(entity => new DanhMucNguonVonDto() {
                Id = entity.Id,
                Ma = entity.Ma ?? string.Empty,
                Ten = entity.Ten ?? string.Empty,
                MoTa = entity.MoTa ?? string.Empty,
                Stt = entity.Stt,
                Used = entity.Used,
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}