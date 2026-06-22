using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.PhanKhaiKinhPhis.Queries;

public record PhanKhaiKinhPhiGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PhanKhaiKinhPhiDto>> {
    public Guid? DuAnId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class PhanKhaiKinhPhiGetDanhSachQueryHandler : IRequestHandler<PhanKhaiKinhPhiGetDanhSachQuery, PaginatedList<PhanKhaiKinhPhiDto>> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public PhanKhaiKinhPhiGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<PhanKhaiKinhPhiDto>> Handle(PhanKhaiKinhPhiGetDanhSachQuery request, CancellationToken cancellationToken = default) {
        var queryable = _repo.GetQueryableSet().AsNoTracking()
            .Include(e => e.TrangThai)
            .Include(e => e.NguonVon)
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
                TrichYeu = e.TrichYeu,
                ThuyetMinh = e.ThuyetMinh,
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
