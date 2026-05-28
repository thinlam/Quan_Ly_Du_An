using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Queries;

public record ToTrinhThamDinhNhaThauDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<ToTrinhThamDinhNhaThauDto>> {
 
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

}

internal class    ToTrinhThamDinhNhaThauDanhSachQueryHandler(IServiceProvider ServiceProvider)    : IRequestHandler<ToTrinhThamDinhNhaThauDanhSachQuery, PaginatedList<ToTrinhThamDinhNhaThauDto>> {
    private readonly IRepository<ToTrinhThamDinhNhaThau, Guid> ToTrinhThamDinhNhaThau =  ServiceProvider.GetRequiredService<IRepository<ToTrinhThamDinhNhaThau, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<ToTrinhThamDinhNhaThauDto>> Handle(ToTrinhThamDinhNhaThauDanhSachQuery request,
        CancellationToken cancellationToken = default) {

        DateTimeOffset? tuNgayDto = null;
        DateTimeOffset? denNgayExclusiveDto = null; 
        if (request.TuNgay.HasValue) {
            var dt = request.TuNgay.Value.ToDateTime(TimeOnly.MinValue);
            tuNgayDto = new DateTimeOffset(dt);
        }
        if (request.DenNgay.HasValue) {
            var dt = request.DenNgay.Value.ToDateTime(TimeOnly.MinValue);
            denNgayExclusiveDto = new DateTimeOffset(dt).AddDays(1);
        }

        var queryable = ToTrinhThamDinhNhaThau.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId != null, e => e.BuocId == request.BuocId)
            .WhereIf(request.So != null, e => e.So.Contains(request.So))
            .WhereIf(request.TrangThaiDangTaiId != null, e => e.TrangThaiDangTaiId == request.TrangThaiDangTaiId)
            .WhereIf(tuNgayDto != null, e => e.NgayTrinh >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayTrinh < denNgayExclusiveDto);
        return await queryable
            .Select(e => new ToTrinhThamDinhNhaThauDto() {
                Id = e.Id,
                DuAnId=e.DuAnId,
                BuocId=e.BuocId,
                So = e.So,
                NgayTrinh = e.NgayTrinh,
                TrichYeu = e.TrichYeu,
                TrangThaiDangTaiId = e.TrangThaiDangTaiId,
                DaThamDinh = e.DaThamDinh,
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