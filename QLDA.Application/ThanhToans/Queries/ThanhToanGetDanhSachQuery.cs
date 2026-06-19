using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ThanhToans.DTOs;

namespace QLDA.Application.ThanhToans.Queries;

public record ThanhToanGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<ThanhToanDto>>
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
    ThanhToanGetDanhSachQueryHandler : IRequestHandler<ThanhToanGetDanhSachQuery,
    PaginatedList<ThanhToanDto>>
{
    private readonly IRepository<ThanhToan, Guid> _thanhToan;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationContext _authContext;

    public ThanhToanGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PaginatedList<ThanhToanDto>> Handle(ThanhToanGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {

        var queryable = _buocAuth.FilterVisibleChildEntities(_thanhToan.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.HopDongId != null, e => e.NghiemThu!.HopDongId == request.HopDongId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.SoHoaDon,
                e => e.NoiDung
            );

        return await queryable
            .Select(x => new ThanhToanDto()
            {
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