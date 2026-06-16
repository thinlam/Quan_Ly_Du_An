using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Extensions;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.TamUngs.Queries;

public record TamUngGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<TamUngDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? HopDongId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class
    TamUngGetDanhSachQueryHandler : IRequestHandler<TamUngGetDanhSachQuery,
    PaginatedList<TamUngDto>> {
    private readonly IRepository<TamUng, Guid> TamUng;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _user;

    public TamUngGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        TamUng = serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
    }

    public async Task<PaginatedList<TamUngDto>> Handle(TamUngGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = TamUng.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.HopDongId != null, e => e.HopDongId == request.HopDongId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.SoPhieuChi,
                e => e.NoiDung,
                e => e.HopDong!.Ten
            )
            .WhereFilterBuocVisibility(_duAnBuocRepo, _auth, _user, e => e.BuocId);

        return await queryable
            .Select(e => new TamUngDto() {
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