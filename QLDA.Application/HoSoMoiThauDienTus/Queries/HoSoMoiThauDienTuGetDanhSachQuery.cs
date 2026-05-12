using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;

namespace QLDA.Application.HoSoMoiThauDienTus.Queries;

public record HoSoMoiThauDienTuGetDanhSachQuery(HoSoMoiThauDienTuSearchDto SearchDto) 
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<HoSoMoiThauDienTuDto>> {
    public string? GlobalFilter { get; set; }
}

internal class HoSoMoiThauDienTuGetDanhSachQueryHandler : IRequestHandler<HoSoMoiThauDienTuGetDanhSachQuery, PaginatedList<HoSoMoiThauDienTuDto>> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;

    public HoSoMoiThauDienTuGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
    }

    public async Task<PaginatedList<HoSoMoiThauDienTuDto>> Handle(HoSoMoiThauDienTuGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = HoSoMoiThauDienTu.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
            .Include(e => e.HinhThucLuaChonNhaThau)
            .Include(e => e.GoiThau)
            .Include(e => e.TrangThaiPheDuyet)
            .WhereGlobalFilter(
                request,  // Truyền request (implement IMayHaveGlobalFilter)
                e => e.ThoiGianThucHien
            );

        // Filter by DuAnId if provided
        if (request.SearchDto.DuAnId.HasValue) {
            queryable = queryable.Where(e => e.DuAnId == request.SearchDto.DuAnId);
        }

        // Filter by GoiThauId if provided
        if (request.SearchDto.GoiThauId.HasValue) {
            queryable = queryable.Where(e => e.GoiThauId == request.SearchDto.GoiThauId);
        }

        return await queryable
            .Select(e => e.ToDto())
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}