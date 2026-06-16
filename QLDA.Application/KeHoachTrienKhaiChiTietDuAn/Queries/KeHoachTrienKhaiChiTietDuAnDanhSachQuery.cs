using Azure.Core;
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Extensions;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Queries;

public record KeHoachTrienKhaiChiTietDuAnDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<KeHoachTrienKhaiChiTietDuAnDto>>
{

    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public string? MaMoc { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? TrangThaiId { get; set; }

    public int? DonViChuTriId  { get; set; }

    public string? Ten { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }

}

internal class KeHoachTrienKhaiChiTietDuAnDanhSachQueryHandler(IServiceProvider ServiceProvider) : IRequestHandler<KeHoachTrienKhaiChiTietDuAnDanhSachQuery, PaginatedList<KeHoachTrienKhaiChiTietDuAnDto>>
{
    private readonly IRepository<Domain.Entities.KeHoachTrienKhaiChiTietDuAn, Guid> KeHoachTrienKhaiChiTietDuAn = ServiceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
    private readonly IRepository<DuAn, Guid> DuAn = ServiceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<Domain.Entities.TepDinhKem, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<Domain.Entities.TepDinhKem, Guid>>();
    private readonly IRepository<DmDonVi, long> DmDonVi = ServiceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo = ServiceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _auth = ServiceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<KeHoachTrienKhaiChiTietDuAnDto>> Handle(KeHoachTrienKhaiChiTietDuAnDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {

        var _dmDonVi = DmDonVi.GetQueryableSet().AsNoTracking();

        var queryable = KeHoachTrienKhaiChiTietDuAn.GetQueryableSet().AsNoTracking()

        .Where(e => !e.IsDeleted)
            .WhereFilterBuocVisibility(_duAnBuocRepo, _auth, User, e => e.BuocId)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId != null, e => e.BuocId == request.BuocId)
            .WhereIf(request.Ten != null, e => e.Ten.Contains(request.Ten))
            .WhereIf(!string.IsNullOrEmpty(request.MaMoc), e => e.MaMoc.Contains(request.MaMoc))
            .WhereIf(request.DonViChuTriId != null, e => e.DonViChuTriId == request.DonViChuTriId)
            .WhereIf(request.TrangThaiId != null, e => e.TrangThaiId == request.TrangThaiId)
            .WhereIf(request.TuNgay != null, e => e.NgayBatDauKeHoach >= request.TuNgay)
            .WhereIf(request.DenNgay != null, e => e.NgayKetThucKeHoach <= request.DenNgay)
            .GroupJoin( DmDonVi.GetQueryableSet().AsNoTracking(),  kh => kh.DonViChuTriId, dv => dv.Id,
                    (kh, dvs) => new {
                        KeHoach = kh,
                        DonVi = dvs.FirstOrDefault()
                    });
        return await queryable
            .Select(e => new KeHoachTrienKhaiChiTietDuAnDto()
            {
                Id = e.KeHoach.Id,
                DuAnId = e.KeHoach.DuAnId,
                BuocId = e.KeHoach.BuocId,
                TenDuAn = e.KeHoach.DuAn != null  ? e.KeHoach.DuAn.TenDuAn  : string.Empty,
                Ten = e.KeHoach.Ten,
                MaMoc = e.KeHoach.MaMoc,
                TrangThaiId = e.KeHoach.TrangThaiId,
                TenTrangThai = e.KeHoach.TrangThaiXuLy != null  ? e.KeHoach.TrangThaiXuLy.Ten : string.Empty,
                TiLeHoanThanh = e.KeHoach.TiLeHoanThanh,
                TenDonViChuTri = e.DonVi != null ? e.DonVi.TenDonVi  : string.Empty,
                NgayBatDauKeHoach = e.KeHoach.NgayBatDauKeHoach,
                NgayBatDauThucTe = e.KeHoach.NgayBatDauThucTe,
                NgayKetThucKeHoach = e.KeHoach.NgayKetThucKeHoach,
                NgayKetThucThucTe = e.KeHoach.NgayKetThucThucTe,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.KeHoach.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
