using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhPheDuyets.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhPheDuyets.Queries;

public record ToTrinhPheDuyetGetPaginatedQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<ToTrinhPheDuyetDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }

    public string? Loai { get; set; }
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
}

internal class
    ToTrinhPheDuyetGetPaginatedQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<ToTrinhPheDuyetGetPaginatedQuery,  PaginatedList<ToTrinhPheDuyetDto>> {
    private readonly IRepository<ToTrinhPheDuyet, Guid> ToTrinhPheDuyet =  ServiceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<ToTrinhPheDuyetDto>> Handle(ToTrinhPheDuyetGetPaginatedQuery request,
        CancellationToken cancellationToken = default) {

        var queryable = ToTrinhPheDuyet.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.Loai != null, e => e.Loai == request.Loai)
            .WhereIf(request.TrichYeu.IsNotNullOrWhitespace(),
                e => e.TrichYeu!.ToLower().Contains(request.TrichYeu!.ToLower()))
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.TuNgay.HasValue, e => e.NgayToTrinh.HasValue && e.NgayToTrinh.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue, e => e.NgayToTrinh.HasValue && e.NgayToTrinh.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(
                request,
                e => e.TrichYeu
            );

        return await queryable
            .Select(e => new ToTrinhPheDuyetDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                So = e.So ?? string.Empty,
                TrichYeu = e.TrichYeu,
                NgayToTrinh = e.NgayToTrinh,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,

                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}