using QLDA.Application.Authorization;

using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.TamUngs.Queries;

public record TamUngGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<TamUngDto>>
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? HopDongId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class
    TamUngGetDanhSachQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<TamUngGetDanhSachQuery,
    PaginatedList<TamUngDto>>
{
    private readonly IRepository<TamUng, Guid> _tamUng = serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();
    private readonly IRepository<Attachment, Guid> TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<PaginatedList<TamUngDto>> Handle(TamUngGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _buocAuth.FilterVisibleChildEntities(_tamUng.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.HopDongId != null, e => e.HopDongId == request.HopDongId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.SoPhieuChi,
                e => e.NoiDung,
                e => e.HopDong!.Ten
            );

        return await queryable
            .Select(e => new TamUngDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                HopDongId = e.HopDongId,
                SoPhieuChi = e.SoPhieuChi,
                GiaTri = e.GiaTri,
                NoiDung = e.NoiDung,
                NgayTamUng = e.NgayTamUng,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}