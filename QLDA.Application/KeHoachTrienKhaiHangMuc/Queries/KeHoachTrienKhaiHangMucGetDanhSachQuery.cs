using Azure.Core;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Queries;

public record KeHoachTrienKhaiHangMucDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<KeHoachTrienKhaiHangMucDto>> {
 
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public string? So { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
      
    public string? TenHangMuc{ get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }

}

internal class    KeHoachTrienKhaiHangMucDanhSachQueryHandler(IServiceProvider ServiceProvider)    : IRequestHandler<KeHoachTrienKhaiHangMucDanhSachQuery, PaginatedList<KeHoachTrienKhaiHangMucDto>> {
    private readonly IRepository<Domain.Entities.KeHoachTrienKhaiHangMuc, Guid> KeHoachTrienKhaiHangMuc =  ServiceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly IRepository<GoiThau, Guid> GoiThau = ServiceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<KeHoachTrienKhaiHangMucDto>> Handle(KeHoachTrienKhaiHangMucDanhSachQuery request,
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

        var queryable = KeHoachTrienKhaiHangMuc.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId != null, e => e.BuocId == request.BuocId)
            .WhereIf(request.So != null, e => e.So.Contains(request.So))
           // .WhereIf(request.TenHangMuc != null, e => e.TenHangMuc.Contains(request.TenHangMuc))
            .WhereIf(tuNgayDto != null, e => e.NgayToTrinh >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayToTrinh < denNgayExclusiveDto);
        return await queryable
            .Select(e => new KeHoachTrienKhaiHangMucDto() {
                Id = e.Id,
                DuAnId=e.DuAnId,
                BuocId=e.BuocId,
                So = e.So,
                NgayTrinh = e.NgayToTrinh,
                TrichYeu = e.TrichYeu,
                TrangThaiId = e.TrangThaiId,    
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : string.Empty,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : string.Empty,
                SoHangMuc = e.DanhSachHangMuc.Count(),
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString() )
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
 