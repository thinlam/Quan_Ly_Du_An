using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ThoaThuanGiaoViecs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ThoaThuanGiaoViecs.Queries;

public record ThoaThuanGiaoViecGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<ThoaThuanGiaoViecDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public DateOnly? TuNgay { get; set; } 
    public DateOnly? DenNgay { get; set; }
}

internal class    ThoaThuanGiaoViecGetDanhSachQueryHandler(IServiceProvider ServiceProvider)    : IRequestHandler<ThoaThuanGiaoViecGetDanhSachQuery, PaginatedList<ThoaThuanGiaoViecDto>> {
    private readonly IRepository<ThoaThuanGiaoViec, Guid> ThoaThuanGiaoViec =
        ServiceProvider.GetRequiredService<IRepository<ThoaThuanGiaoViec, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<ThoaThuanGiaoViecDto>> Handle(ThoaThuanGiaoViecGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {

        var queryable = ThoaThuanGiaoViec.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId);
           // .WhereIf(request.TuNgay.HasValue, e => e.CreatedAt.HasValue && e.CreatedAt.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
          //  .WhereIf(request.DenNgay.HasValue, e => e.NgayBatDauDuKien.HasValue && e.NgayBatDauDuKien.Value <= request.DenNgay!.Value.ToEndOfDayUtc());
            
        return await queryable
            .Select(e => new ThoaThuanGiaoViecDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                GoiThauId = e.GoiThauId,

                PhamVi = e.PhamVi,
                ChatLuong = e.ChatLuong,
                NoiDung = e.NoiDung,
                ThoiGian = e.ThoiGian,    
                GiaTri = e.GiaTri,    
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,   
                TenGoiThau = e.GoiThau != null ? e.GoiThau.Ten : null, 
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