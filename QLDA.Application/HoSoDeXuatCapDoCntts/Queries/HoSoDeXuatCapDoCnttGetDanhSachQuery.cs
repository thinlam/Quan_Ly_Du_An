using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Queries;

public record HoSoDeXuatCapDoCnttGetDanhSachQuery(HoSoDeXuatCapDoCnttSearchDto SearchDto)
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<HoSoDeXuatCapDoCnttDto>> {
    public string? GlobalFilter { get; set; }
}

internal class HoSoDeXuatCapDoCnttGetDanhSachQueryHandler 
    : IRequestHandler<HoSoDeXuatCapDoCnttGetDanhSachQuery, PaginatedList<HoSoDeXuatCapDoCnttDto>> {
    
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> HoSoDeXuatCapDoCntt;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public HoSoDeXuatCapDoCnttGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        HoSoDeXuatCapDoCntt = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<HoSoDeXuatCapDoCnttDto>> Handle(HoSoDeXuatCapDoCnttGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        
        var queryable = HoSoDeXuatCapDoCntt.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.CapDo)
            .WhereIf(request.SearchDto.DuAnId.HasValue, e => e.DuAnId == request.SearchDto.DuAnId)
            .WhereIf(request.SearchDto.BuocId.HasValue, e => e.BuocId == request.SearchDto.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.NoiDungDeNghi,
                e => e.NoiDungBaoCao,
                e => e.NoiDungDuThao
            );

        var dtos = await queryable
            .Select(e => e.ToDto())
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);

        // Load file đính kèm cho từng item
        var groupIds = dtos.Data.Select(d => d.Id.ToString()).ToList();
        if (groupIds.Count > 0) {
            var files = await TepDinhKem.GetQueryableSet()
                .AsNoTracking()
                .Where(f => groupIds.Contains(f.GroupId))
                .ToListAsync(cancellationToken);

            foreach (var dto in dtos.Data) {
                dto.DanhSachTepDinhKem = files.Where(f => f.GroupId == dto.Id.ToString())
                    .Select(f => f.ToDto()).ToList();
            }
        }

        return dtos;
    }
}