using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TrienKhaiKeHoachLCNTs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.TrienKhaiKeHoachLCNTs.Queries;

public record TrienKhaiKeHoachLCNTDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<TrienKhaiKeHoachLCNTDto>>
{

    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public long? PhongBanDeXuatId { get; set; }
    public long? NguoiDeXuatId { get; set; }
    public string? So { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? GoiThauId { get; set; }

    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? TrangThaiDangTaiId { get; set; }

}

internal class TrienKhaiKeHoachLCNTDanhSachQueryHandler(IServiceProvider ServiceProvider) : IRequestHandler<TrienKhaiKeHoachLCNTDanhSachQuery, PaginatedList<TrienKhaiKeHoachLCNTDto>>
{
    private readonly IRepository<Domain.Entities.TrienKhaiKeHoachLCNT, Guid> TrienKhaiKeHoachLCNT = ServiceProvider.GetRequiredService<IRepository<TrienKhaiKeHoachLCNT, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly IRepository<GoiThau, Guid> GoiThau = ServiceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo = ServiceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth = ServiceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext = ServiceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<PaginatedList<TrienKhaiKeHoachLCNTDto>> Handle(TrienKhaiKeHoachLCNTDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {

        DateTimeOffset? tuNgayDto = null;
        DateTimeOffset? denNgayExclusiveDto = null;
        if (request.TuNgay.HasValue)
        {
            var dt = request.TuNgay.Value.ToDateTime(TimeOnly.MinValue);
            tuNgayDto = new DateTimeOffset(dt);
        }
        if (request.DenNgay.HasValue)
        {
            var dt = request.DenNgay.Value.ToDateTime(TimeOnly.MinValue);
            denNgayExclusiveDto = new DateTimeOffset(dt).AddDays(1);
        }

        var queryable = TrienKhaiKeHoachLCNT.GetQueryableSet().AsNoTracking()
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId != null, e => e.BuocId == request.BuocId)
            .WhereIf(request.So != null, e => e.So.Contains(request.So!))
            .WhereIf(request.TrangThaiDangTaiId != null, e => e.TrangThaiDangTaiId == request.TrangThaiDangTaiId)
            .WhereIf(tuNgayDto != null, e => e.NgayTrinh >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayTrinh < denNgayExclusiveDto);

        queryable = _buocAuth.FilterVisibleChildEntities(queryable, _duAnBuocRepo, _authContext, e => e.BuocId);
        return await queryable
            .Select(e => new TrienKhaiKeHoachLCNTDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                So = e.So,
                NgayTrinh = e.NgayTrinh,
                TrichYeu = e.TrichYeu,
                TrangThaiDangTaiId = e.TrangThaiDangTaiId,
                HinhThucLCNT = e.HinhThucLCNT,
                NoiDung = e.NoiDung,
                YeuCau = e.YeuCau,
                ThoiGianThucHien = e.ThoiGianThucHien,
                GiaTri = e.GiaTri,
                GoiThauId = e.GoiThauId,
                // trả thêm tên dự án
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : string.Empty,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : string.Empty,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}