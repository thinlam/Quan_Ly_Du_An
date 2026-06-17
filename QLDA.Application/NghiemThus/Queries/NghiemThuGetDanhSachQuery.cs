using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

using QLDA.Application.Common.Mapping;
using QLDA.Application.Providers;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.NghiemThus.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.NghiemThus.Queries;

public record NghiemThuGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<NghiemThuDto>>
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? HopDongId { get; set; }
    public Guid? ThanhToanId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    public bool IsCbo { get; set; }
}

internal class
    NghiemThuGetDanhSachQueryHandler : IRequestHandler<NghiemThuGetDanhSachQuery,
    PaginatedList<NghiemThuDto>>
{
    private readonly IRepository<NghiemThu, Guid> NghiemThu;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationContext _authContext;

    public NghiemThuGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        NghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PaginatedList<NghiemThuDto>> Handle(NghiemThuGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {

        var queryable = _buocAuth.FilterVisibleChildEntities(NghiemThu.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.HopDongId != null, e => e.HopDongId == request.HopDongId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.SoBienBan,
                e => e.NoiDung
            );


        return await queryable
            .Select(e => new NghiemThuDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                HopDongId = e.HopDongId,
                PhuLucHopDongIds = e.NghiemThuPhuLucHopDongs!.Select(junction => junction.RightId).ToList(),
                Dot = e.Dot,
                Ngay = e.Ngay,
                NoiDung = e.NoiDung,
                SoBienBan = e.SoBienBan,
                //ThanhToanId = !e.ThanhToan!.IsDeleted ? e.ThanhToan!.Id : null,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}