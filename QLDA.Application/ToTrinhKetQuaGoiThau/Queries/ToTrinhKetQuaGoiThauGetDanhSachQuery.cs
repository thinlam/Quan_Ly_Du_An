using QLDA.Application.Authorization;

using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhKetQuaGoiThaus.DTOs;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.Queries;

public record ToTrinhKetQuaGoiThauDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<ToTrinhKetQuaGoiThauDto>>
{

    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public long? PhongBanDeXuatId { get; set; }
    public long? NguoiDeXuatId { get; set; }
    public string? So { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }

    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }

}

internal class ToTrinhKetQuaGoiThauDanhSachQueryHandler(IServiceProvider ServiceProvider) : IRequestHandler<ToTrinhKetQuaGoiThauDanhSachQuery, PaginatedList<ToTrinhKetQuaGoiThauDto>>
{
    private readonly IRepository<ToTrinhKetQuaGoiThau, Guid> _toTrinhKetQuaGoiThau = ServiceProvider.GetRequiredService<IRepository<ToTrinhKetQuaGoiThau, Guid>>();

    private readonly IRepository<Attachment, Guid> _tepDinhKem = ServiceProvider.GetRequiredService<IRepository<Attachment, Guid>>();

    private readonly IAuthorizationContext _authContext = ServiceProvider.GetRequiredService<IAuthorizationContext>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo = ServiceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth = ServiceProvider.GetRequiredService<IBuocAuthorizationProvider>();

    public async Task<PaginatedList<ToTrinhKetQuaGoiThauDto>> Handle(ToTrinhKetQuaGoiThauDanhSachQuery request,
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
        var queryable = _buocAuth.FilterVisibleChildEntities(_toTrinhKetQuaGoiThau.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.BuocId != null, e => e.BuocId == request.BuocId)
            .WhereIf(request.So != null, e => e.So.Contains(request.So!))
            .WhereIf(request.TrangThaiDangTaiId != null, e => e.TrangThaiDangTaiId == request.TrangThaiDangTaiId)
            .WhereIf(tuNgayDto != null, e => e.NgayTrinh >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayTrinh < denNgayExclusiveDto);
        return await queryable
            .Select(e => new ToTrinhKetQuaGoiThauDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                So = e.So,
                NgayTrinh = e.NgayTrinh,
                TrichYeu = e.TrichYeu,
                TrangThaiDangTaiId = e.TrangThaiDangTaiId,
                // trả thêm tên dự án
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ma : string.Empty,
                TenTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ten : string.Empty,
                DanhSachTepDinhKem = _tepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}