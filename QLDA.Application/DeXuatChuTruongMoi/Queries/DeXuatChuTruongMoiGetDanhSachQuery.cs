using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.DeXuatChuTruongMois.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatChuTruongMois.Queries;

public record DeXuatChuTruongMoiQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<DeXuatChuTruongMoiDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }

    public int? HinhThucDauTuId { get; set; }
    public long? LanhDaoPhuTrachId { get; set; }
    public long? DonViPhuTrachId { get; set; }
    public DateOnly? TuNgay { get; set; } // by NgayBatDauDuKien
    public DateOnly? DenNgay { get; set; }
}

internal class
    DeXuatChuTruongMoiQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DeXuatChuTruongMoiQuery, PaginatedList<DeXuatChuTruongMoiDto>> {
    private readonly IRepository<DeXuatChuTruongMoi, Guid> DeXuatChuTruongMoi =
        ServiceProvider.GetRequiredService<IRepository<DeXuatChuTruongMoi, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<DeXuatChuTruongMoiDto>> Handle(DeXuatChuTruongMoiQuery request,
        CancellationToken cancellationToken = default) {
        bool dieuKienThayTatCa = false;

        var queryable = DeXuatChuTruongMoi.GetQueryableSet().AsNoTracking()
            .WhereIf(User.Id > 0 && !dieuKienThayTatCa, e => e.CreatedBy == User.Id.ToString(), e => dieuKienThayTatCa)
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.HinhThucDauTuId != null, e => e.HinhThucDauTuId == request.HinhThucDauTuId)
            .WhereIf(request.LanhDaoPhuTrachId != null, e => e.LanhDaoPhuTrachId == request.LanhDaoPhuTrachId)
            .WhereIf(request.DonViPhuTrachId != null, e => e.DonViPhuTrachChinhId == request.DonViPhuTrachId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.TuNgay.HasValue, e => e.NgayBatDauDuKien.HasValue && e.NgayBatDauDuKien.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue, e => e.NgayBatDauDuKien.HasValue && e.NgayBatDauDuKien.Value <= request.DenNgay!.Value.ToEndOfDayUtc());
            
        return await queryable
            .Select(e => new DeXuatChuTruongMoiDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                TomTatNoiDung = e.TomTatNoiDung,
                HinhThucDauTuId = e.HinhThucDauTuId,
                NgayBatDauDuKien = e.NgayBatDauDuKien,
                LanhDaoPhuTrachId = e.LanhDaoPhuTrachId,    
                DonViPhuTrachChinhId = e.DonViPhuTrachChinhId,    
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