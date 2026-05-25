using Microsoft.EntityFrameworkCore;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Application.Common.Mapping;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Queries;

public record BaoCaoKetQuaKhaoSatGetDanhSachQuery(BaoCaoKetQuaKhaoSatSearchDto SearchDto)
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<BaoCaoKetQuaKhaoSatDto>>
{
    public string? GlobalFilter { get; set; }
}

internal class BaoCaoKetQuaKhaoSatGetDanhSachQueryHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatGetDanhSachQuery, PaginatedList<BaoCaoKetQuaKhaoSatDto>>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;

    public BaoCaoKetQuaKhaoSatGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
    }

    public async Task<PaginatedList<BaoCaoKetQuaKhaoSatDto>> Handle(
        BaoCaoKetQuaKhaoSatGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _repository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Include(e => e.TrangThai)
            .WhereIf(request.SearchDto.DuAnId.HasValue, e => e.DuAnId == request.SearchDto.DuAnId)
            .WhereIf(request.SearchDto.BuocId.HasValue, e => e.BuocId == request.SearchDto.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.NoiDungBaoCao,
                e => e.NoiDungNghiemThu);

        return await queryable
            .Select(e => e.ToDto())
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}
