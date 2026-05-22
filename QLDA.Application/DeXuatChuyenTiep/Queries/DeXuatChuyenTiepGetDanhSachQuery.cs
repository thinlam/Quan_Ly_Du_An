using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.DeXuatChuyenTieps.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatChuyenTieps.Queries;

public record DeXuatChuyenTiepGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<DeXuatChuyenTiepDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public DateOnly? TuNgay { get; set; } 
    public DateOnly? DenNgay { get; set; }
}

internal class    DeXuatChuyenTiepGetDanhSachQueryHandler(IServiceProvider ServiceProvider)    : IRequestHandler<DeXuatChuyenTiepGetDanhSachQuery, PaginatedList<DeXuatChuyenTiepDto>> {
    private readonly IRepository<DeXuatChuyenTiep, Guid> DeXuatChuyenTiep =
        ServiceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<DeXuatChuyenTiepDto>> Handle(DeXuatChuyenTiepGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        bool dieuKienThayTatCa = false;

        var queryable = DeXuatChuyenTiep.GetQueryableSet().AsNoTracking()
            .WhereIf(User.Id > 0 && !dieuKienThayTatCa, e => e.CreatedBy == User.Id.ToString(), e => dieuKienThayTatCa)
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId);
           // .WhereIf(request.TuNgay.HasValue, e => e.CreatedAt.HasValue && e.CreatedAt.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
          //  .WhereIf(request.DenNgay.HasValue, e => e.NgayBatDauDuKien.HasValue && e.NgayBatDauDuKien.Value <= request.DenNgay!.Value.ToEndOfDayUtc());
            
        return await queryable
            .Select(e => new DeXuatChuyenTiepDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                UocGiaiNgan = e.UocGiaiNgan,
                SoLieuGiaiNgan = e.SoLieuGiaiNgan,
                KhoiLuongThucTe = e.KhoiLuongThucTe,
                KhoiLuongDuKien = e.KhoiLuongDuKien,    
                NhuCauKinhPhi = e.NhuCauKinhPhi,    
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