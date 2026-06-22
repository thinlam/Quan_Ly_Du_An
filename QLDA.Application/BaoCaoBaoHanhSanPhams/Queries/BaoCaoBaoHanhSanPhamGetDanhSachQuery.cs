using Microsoft.EntityFrameworkCore;
using QLDA.Application.BaoCaoBaoHanhSanPhams.DTOs;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.BaoCaoBaoHanhSanPhams.Queries;

public record BaoCaoBaoHanhSanPhamGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<BaoCaoBaoHanhSanPhamDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }

    public string? NoiDung { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class
    BaoCaoBaoHanhSanPhamGetDanhSachQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<BaoCaoBaoHanhSanPhamGetDanhSachQuery,
        PaginatedList<BaoCaoBaoHanhSanPhamDto>> {
    private readonly IRepository<BaoCaoBaoHanhSanPham, Guid> BaoCaoBaoHanhSanPham =
        ServiceProvider.GetRequiredService<IRepository<BaoCaoBaoHanhSanPham, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<BaoCaoBaoHanhSanPhamDto>> Handle(BaoCaoBaoHanhSanPhamGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        bool dieuKienThayTatCa = false;

        var queryable = BaoCaoBaoHanhSanPham.GetQueryableSet().AsNoTracking()
            .WhereIf(User.Id > 0 && !dieuKienThayTatCa, e => e.CreatedBy == User.Id.ToString(), e => dieuKienThayTatCa)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.NoiDung.IsNotNullOrWhitespace(),
                e => e.NoiDung!.ToLower().Contains(request.NoiDung!.ToLower()))
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.TuNgay.HasValue, e => e.Ngay.HasValue && e.Ngay.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue, e => e.Ngay.HasValue && e.Ngay.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(
                request,
                e => e.NoiDung
            );

        return await queryable
            .Select(e => new BaoCaoBaoHanhSanPhamDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                NoiDung = e.NoiDung,
                Ngay = e.Ngay,
                NguoiBaoCaoId = long.Parse(e.CreatedBy),
                LanhDaoPhuTrachId = e.LanhDaoPhuTrachId,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}