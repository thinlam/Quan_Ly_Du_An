using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DanhMucNhaThaus.DTOs;

namespace QLDA.Application.DanhMucNhaThaus.Queries;

public record DanhMucNhaThauGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<DanhMucNhaThauDto>> {
    public bool GetAll { get; set; }
    public string? DuAnId { get; set; }
    public bool IsTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public List<Guid>? Ids { get; set; }
}

public record DanhMucNhaThauGetDanhSachQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DanhMucNhaThauGetDanhSachQuery, PaginatedList<DanhMucNhaThauDto>> {
    private readonly IRepository<DanhMucNhaThau, Guid> DanhMucNhaThau =
        ServiceProvider.GetRequiredService<IRepository<DanhMucNhaThau, Guid>>();

    public async Task<PaginatedList<DanhMucNhaThauDto>> Handle(DanhMucNhaThauGetDanhSachQuery request,
        CancellationToken cancellationToken) {
        var query = DanhMucNhaThau.GetOrderedSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .WhereIf(request.Ids != null, e => request.Ids!.Contains(e.Id) || e.Used || request.GetAll, e => request.GetAll || e.Used)
            .WhereGlobalFilter(
                request,
                e => e.Ten,
                e => e.MoTa,
                e => e.DiaChi,
                e => e.MaSoThue,
                e => e.Email,
                e => e.SoDienThoai,
                e => e.NguoiDaiDien
            )
            .WhereFunc(request.IsTracking, e => e.AsNoTracking());


        return await query
            .Select(entity => new DanhMucNhaThauDto() {
                Id = entity.Id,
                Ma = entity.Ma ?? string.Empty,
                Ten = entity.Ten ?? string.Empty,
                MoTa = entity.MoTa ?? string.Empty,
                Stt = entity.Stt,
                Used = entity.Used,
                DiaChi = entity.DiaChi,
                MaSoThue = entity.MaSoThue,
                Email = entity.Email,
                SoDienThoai = entity.SoDienThoai,
                NguoiDaiDien = entity.NguoiDaiDien,
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}