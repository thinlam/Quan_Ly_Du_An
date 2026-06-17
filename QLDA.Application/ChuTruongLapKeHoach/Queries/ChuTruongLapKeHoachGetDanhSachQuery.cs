using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ChuTruongLapKeHoachs.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;

namespace QLDA.Application.ChuTruongLapKeHoachs.Queries;

public record ChuTruongLapKeHoachDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<ChuTruongLapKeHoachDto>>
{
    public Guid? DuAnId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? So { get; set; }
}

internal class
    ChuTruongLapKeHoachDanhSachQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<ChuTruongLapKeHoachDanhSachQuery,
    PaginatedList<ChuTruongLapKeHoachDto>>
{
    private readonly IRepository<ChuTruongLapKeHoach, Guid> _chuTruongLapKeHoach = serviceProvider.GetRequiredService<IRepository<ChuTruongLapKeHoach, Guid>>();
    private readonly IRepository<TepDinhKem, Guid> _tepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<PaginatedList<ChuTruongLapKeHoachDto>> Handle(ChuTruongLapKeHoachDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _buocAuth.FilterVisibleChildEntities(_chuTruongLapKeHoach.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.TuNgay != null, e => e.NgayToTrinh >= request.TuNgay.ToStartOfDayUtc())
            .WhereIf(request.DenNgay != null, e => e.NgayToTrinh <= request.DenNgay.ToEndOfDayUtc())
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereGlobalFilter(
                request,
                e => e.SoToTrinh
            );

        return await queryable
            .Select(e => new ChuTruongLapKeHoachDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "Không rõ",
                TrangThaiId = e.TrangThaiId,
                TenTrangThai = e.TrangThai != null ? e.TrangThai.Ten : string.Empty,

                LoaiDeXuat = e.LoaiDeXuat,
                TenLoaiDeXuat = e.LoaiDeXuat == (int)LoaiDeXuatLCNTonstants.LoaiDeXuatMacDinh.KhongLap ?
                LoaiDeXuatLCNTonstants.Default.KhongLap : LoaiDeXuatLCNTonstants.Default.XinChuTruong,
                SoToTrinh = e.SoToTrinh,
                NgayToTrinh = e.NgayToTrinh,
                TrichYeu = e.TrichYeu,
                ButPhe = e.ButPhe,
                DanhSachTepDinhKem = _tepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}