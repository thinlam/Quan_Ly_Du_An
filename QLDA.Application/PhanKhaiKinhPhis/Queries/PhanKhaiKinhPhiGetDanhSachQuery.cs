using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;

namespace QLDA.Application.PhanKhaiKinhPhis.Queries;

public record PhanKhaiKinhPhiGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PhanKhaiKinhPhiDto>> {
    public Guid? DuAnId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class PhanKhaiKinhPhiGetDanhSachQueryHandler : IRequestHandler<PhanKhaiKinhPhiGetDanhSachQuery, PaginatedList<PhanKhaiKinhPhiDto>> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo;

    public PhanKhaiKinhPhiGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
    }

    public async Task<PaginatedList<PhanKhaiKinhPhiDto>> Handle(PhanKhaiKinhPhiGetDanhSachQuery request, CancellationToken cancellationToken = default) {
        var queryable = _repo.GetQueryableSet().AsNoTracking()
            .Include(e => e.TrangThai)
            .Include(e => e.NguonVon)
            .Where(e => !e.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereGlobalFilter(request,
                e => e.SoToTrinh,
                e => e.NguonVon != null ? e.NguonVon.Ten : null
            );

        return await queryable
            .Select(e => new PhanKhaiKinhPhiDto {
                Id = e.Id,
                DuAnId = e.DuAnId,
                SoToTrinh = e.SoToTrinh,
                NgayToTrinh = e.NgayToTrinh,
                NguonVonId = e.NguonVonId,
                TenNguonVon = e.NguonVon != null ? e.NguonVon.Ten : null,
                KinhPhiDeXuat = e.KinhPhiDeXuat,
                KinhPhiPhanKhai = e.KinhPhiPhanKhai,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null ? e.TrangThai.Ma : null,
                TenTrangThai = e.TrangThai != null ? e.TrangThai.Ten : null,
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
