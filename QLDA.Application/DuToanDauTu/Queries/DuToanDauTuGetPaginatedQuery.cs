using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.DuToanDauTus.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.DuToanDauTus.Queries;

public record DuToanDauTuGetPaginatedQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<DuToanDauTuDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }

    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
}

internal class
    DuToanDauTuGetPaginatedQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DuToanDauTuGetPaginatedQuery,  PaginatedList<DuToanDauTuDto>> {
    private readonly IRepository<DuToanDauTu, Guid> DuToanDauTu =  ServiceProvider.GetRequiredService<IRepository<DuToanDauTu, Guid>>();
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<DuToanDauTuDto>> Handle(DuToanDauTuGetPaginatedQuery request,
        CancellationToken cancellationToken = default) {

        var queryable = DuToanDauTu.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.TrichYeu.IsNotNullOrWhitespace(),
                e => e.TrichYeu!.ToLower().Contains(request.TrichYeu!.ToLower()))
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.TuNgay.HasValue, e => e.NgayTrinh.HasValue && e.NgayTrinh.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue, e => e.NgayTrinh.HasValue && e.NgayTrinh.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(
                request,
                e => e.TrichYeu
            );

        return await queryable
            .Select(e => new DuToanDauTuDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                SoToTrinh = e.SoToTrinh ?? string.Empty,
                TrichYeu = e.TrichYeu,
                NgayTrinh = e.NgayTrinh,
                
                TongMucDauTu = e.TongMucDauTu,    
                TongDuToan = e.TongDuToan,
                NguonVonId = e.NguonVonId,
                TenNguonVon = e.NguonVon != null ? e.NguonVon.Ten : string.Empty,
                TenPhuongAnThietKe = e.PhuongAnThietKe != null ? e.PhuongAnThietKe.Ten : string.Empty,  
              
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