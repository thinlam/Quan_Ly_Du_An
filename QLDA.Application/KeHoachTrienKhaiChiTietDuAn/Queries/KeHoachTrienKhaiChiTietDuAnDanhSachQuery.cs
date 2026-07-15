using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Queries;

public record KeHoachTrienKhaiChiTietDuAnDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<KeHoachTrienKhaiChiTietDuAnDto>>
{

    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public string? MaMoc { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? TrangThaiId { get; set; }

    public int? DonViChuTriId { get; set; }

    public string? Ten { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }

}

internal class KeHoachTrienKhaiChiTietDuAnDanhSachQueryHandler(IServiceProvider ServiceProvider) : IRequestHandler<KeHoachTrienKhaiChiTietDuAnDanhSachQuery, PaginatedList<KeHoachTrienKhaiChiTietDuAnDto>>
{
    private readonly IRepository<Domain.Entities.KeHoachTrienKhaiChiTietDuAn, Guid> KeHoachTrienKhaiChiTietDuAn = ServiceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
    private readonly IRepository<DuAn, Guid> DuAn = ServiceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<Attachment, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
    private readonly IRepository<DmDonVi, long> DmDonVi = ServiceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo = ServiceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth = ServiceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext = ServiceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<PaginatedList<KeHoachTrienKhaiChiTietDuAnDto>> Handle(KeHoachTrienKhaiChiTietDuAnDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {

        var _dmDonVi = DmDonVi.GetQueryableSet().AsNoTracking();

        var queryable = _buocAuth.FilterVisibleChildEntities(KeHoachTrienKhaiChiTietDuAn.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.BuocId != null, e => e.BuocId == request.BuocId)
            .WhereIf(request.Ten != null, e => e.Ten!.Contains(request.Ten!))
            .WhereIf(request.MaMoc.IsNotNullOrWhitespace(), e => e.MaMoc.Contains(request.MaMoc!))
            .WhereIf(request.DonViChuTriId != null, e => e.DonViChuTriId == request.DonViChuTriId)
            .WhereIf(request.TrangThaiId != null, e => e.TrangThaiId == request.TrangThaiId)
            .WhereIf(request.TuNgay != null, e => e.NgayBatDauKeHoach >= request.TuNgay)
            .WhereIf(request.DenNgay != null, e => e.NgayKetThucKeHoach <= request.DenNgay)
            .GroupJoin(DmDonVi.GetQueryableSet().AsNoTracking(), kh => kh.DonViChuTriId, dv => dv.Id,
                    (kh, dvs) => new
                    {
                        KeHoach = kh,
                        DonVi = dvs.FirstOrDefault()
                    });
        return await queryable
            .Select(e => new KeHoachTrienKhaiChiTietDuAnDto()
            {
                Id = e.KeHoach.Id,
                DuAnId = e.KeHoach.DuAnId,
                BuocId = e.KeHoach.BuocId,
                TenDuAn = e.KeHoach.DuAn != null ? e.KeHoach.DuAn.TenDuAn : string.Empty,
                Ten = e.KeHoach.Ten,
                MaMoc = e.KeHoach.MaMoc,
                TrangThaiId = e.KeHoach.TrangThaiId,
                TenTrangThai = e.KeHoach.TrangThaiXuLy != null ? e.KeHoach.TrangThaiXuLy.Ten : string.Empty,
                TiLeHoanThanh = e.KeHoach.TiLeHoanThanh,
                TenDonViChuTri = e.DonVi != null ? e.DonVi.TenDonVi : string.Empty,
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
