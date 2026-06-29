using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.PhuLucHopDongs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.PhuLucHopDongs.Queries;

public record PhuLucHopDongGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PhuLucHopDongDto>>, IFromDateToDate
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }


    public string? Ten { get; set; }
    public string? SoPhuLucHopDong { get; set; }
    public string? NoiDung { get; set; }
    public Guid? HopDongId { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class
    PhuLucHopDongGetDanhSachQueryHandler : IRequestHandler<PhuLucHopDongGetDanhSachQuery,
    PaginatedList<PhuLucHopDongDto>>
{
    private readonly IRepository<PhuLucHopDong, Guid> PhuLucHopDong;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationContext _authContext;

    public PhuLucHopDongGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        PhuLucHopDong = serviceProvider.GetRequiredService<IRepository<PhuLucHopDong, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PaginatedList<PhuLucHopDongDto>> Handle(PhuLucHopDongGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _buocAuth.FilterVisibleChildEntities(PhuLucHopDong.GetQueryableSet().AsNoTracking(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.HopDongId != null, e => e.HopDongId == request.HopDongId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.Ten.IsNotNullOrWhitespace(), e => e.Ten!.ToLower().Contains(request.Ten!.ToLower()))
            .WhereIf(request.SoPhuLucHopDong.IsNotNullOrWhitespace(), e => e.SoPhuLucHopDong!.ToLower().Contains(request.SoPhuLucHopDong!.ToLower()))
            .WhereIf(request.NoiDung.IsNotNullOrWhitespace(), e => e.NoiDung!.ToLower().Contains(request.NoiDung!.ToLower()))
            .WhereIf(request.HopDongId != null, e => e.HopDongId == request.HopDongId)
            .WhereIf(request.TuNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(
                request,
                e => e.Ten,
                e => e.NoiDung,
                e => e.SoPhuLucHopDong,
                e => e.HopDong!.Ten
            );

        return await queryable
            .Select(e => new PhuLucHopDongDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                Ten = e.Ten,
                SoPhuLucHopDong = e.SoPhuLucHopDong,
                NoiDung = e.NoiDung,
                Ngay = e.Ngay,
                HopDongId = e.HopDongId,
                GiaTri = e.GiaTri,
                NgayDuKienKetThuc = e.NgayDuKienKetThuc
            }).PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
