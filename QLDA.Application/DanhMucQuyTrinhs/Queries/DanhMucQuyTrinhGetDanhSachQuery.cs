using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DanhMucQuyTrinhs.DTOs;

namespace QLDA.Application.DanhMucQuyTrinhs.Queries;

public record DanhMucQuyTrinhGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<DanhMucQuyTrinhDto>> {
    public bool IsCbo { get; set; }
    public string? GlobalFilter { get; set; }
    public List<int>? Ids { get; set; }
    public bool HasStep { get; set; }
}

public record DanhMucQuyTrinhGetDanhSachQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DanhMucQuyTrinhGetDanhSachQuery, PaginatedList<DanhMucQuyTrinhDto>> {
    private readonly IRepository<DanhMucQuyTrinh, int> DanhMucQuyTrinh =
        ServiceProvider.GetRequiredService<IRepository<DanhMucQuyTrinh, int>>();

    public async Task<PaginatedList<DanhMucQuyTrinhDto>> Handle(DanhMucQuyTrinhGetDanhSachQuery request,
        CancellationToken cancellationToken) {
        var query = DanhMucQuyTrinh.GetOrderedSet().AsNoTracking()
                .WhereIf(request.HasStep, e => e.Buocs!.Any())
                .WhereFunc(request.IsCbo,
                    q => q //Combobox
                           //Còn sử dụng (used = true) và chưa xoá (isDeleted = false) hoặc nằm trong ids
                        .WhereIf(request.Ids != null, e => request.Ids!.Contains(e.Id) || e.Used && !e.IsDeleted, e => e.Used && !e.IsDeleted)
                    , q => q //danh sách
                        .Where(e => !e.IsDeleted) //chưa xoá
                )
                .WhereGlobalFilter(
                    request,
                    e => e.Ten
                )
                ;

        return await query
            .Select(entity => new DanhMucQuyTrinhDto() {
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