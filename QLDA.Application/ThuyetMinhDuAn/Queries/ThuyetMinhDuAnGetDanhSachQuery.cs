using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ThuyetMinhDuAns.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ThuyetMinhDuAns.Queries;

public record ThuyetMinhDuAnGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<ThuyetMinhDuAnDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public DateOnly? TuNgay { get; set; } 
    public DateOnly? DenNgay { get; set; }
}

internal class    ThuyetMinhDuAnGetDanhSachQueryHandler(IServiceProvider ServiceProvider)    : IRequestHandler<ThuyetMinhDuAnGetDanhSachQuery, PaginatedList<ThuyetMinhDuAnDto>> {
    private readonly IRepository<ThuyetMinhDuAn, Guid> ThuyetMinhDuAn = ServiceProvider.GetRequiredService<IRepository<ThuyetMinhDuAn, Guid>>();
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =  ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<ThuyetMinhDuAnDto>> Handle(ThuyetMinhDuAnGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        bool dieuKienThayTatCa = false;

        var queryable = ThuyetMinhDuAn.GetQueryableSet().AsNoTracking()
            .WhereIf(User.Id > 0 && !dieuKienThayTatCa, e => e.CreatedBy == User.Id.ToString(), e => dieuKienThayTatCa)
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId);
           // .WhereIf(request.TuNgay.HasValue, e => e.CreatedAt.HasValue && e.CreatedAt.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
          //  .WhereIf(request.DenNgay.HasValue, e => e.NgayBatDauDuKien.HasValue && e.NgayBatDauDuKien.Value <= request.DenNgay!.Value.ToEndOfDayUtc());
            
        return await queryable
            .Select(e => new ThuyetMinhDuAnDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                So = e.So,
                NgayTrinh = e.NgayTrinh,
                TrichYeu = e.TrichYeu,
                KetQuaThamDinh = e.KetQuaThamDinh,
                TrangThaiThamDinhId = e.TrangThaiThamDinhId,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThaiThamDinh = e.TrangThaiThamDinhId != null  ? e.TrangThaiThamDinh.Ten : string.Empty,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString() && i.GroupType == GroupTypeConstants.ThuyetMinhDuAnThamDinh )
                    .Select(i => i.ToDto()).ToList(),
                DanhSachTepThamDinh = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString() && i.GroupType == GroupTypeConstants.ThuyetMinhDuAnThamDinh)
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}