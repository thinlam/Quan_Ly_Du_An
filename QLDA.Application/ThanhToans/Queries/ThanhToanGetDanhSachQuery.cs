using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Extensions;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ThanhToans.DTOs;

namespace QLDA.Application.ThanhToans.Queries;

public record ThanhToanGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<ThanhToanDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? HopDongId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class
    ThanhToanGetDanhSachQueryHandler : IRequestHandler<ThanhToanGetDanhSachQuery,
    PaginatedList<ThanhToanDto>> {
    private readonly IRepository<ThanhToan, Guid> ThanhToan;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _user;

    public ThanhToanGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        ThanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
    }

    public async Task<PaginatedList<ThanhToanDto>> Handle(ThanhToanGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {

        var queryable = ThanhToan.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.HopDongId != null, e => e.NghiemThu!.HopDongId == request.HopDongId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.SoHoaDon,
                e => e.NoiDung
            )
            .WhereFilterBuocVisibility(_duAnBuocRepo, _auth, _user, e => e.BuocId);

        return await queryable
            .Select(x => new ThanhToanDto() {
                Id = x.Id,
                DuAnId = x.DuAnId,
                BuocId = x.BuocId,
                SoHoaDon = x.SoHoaDon,
                NgayHoaDon = x.NgayHoaDon,
                GiaTri = x.GiaTri,
                NoiDung = x.NoiDung,
                NghiemThuId = x.NghiemThuId,
              //  PhuLucs = x.PhuLucs,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == x.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}