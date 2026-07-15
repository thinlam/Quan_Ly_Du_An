using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.PhanKhaiKinhPhis.Queries;

public record PhanKhaiKinhPhiGetDanhSachQuery(PhanKhaiKinhPhiSearchDto SearchDto)
    : AggregateRootPagination, IRequest<PaginatedList<PhanKhaiKinhPhiDto>> {
    public bool IsNoTracking { get; set; }
}

internal class PhanKhaiKinhPhiGetDanhSachQueryHandler : IRequestHandler<PhanKhaiKinhPhiGetDanhSachQuery, PaginatedList<PhanKhaiKinhPhiDto>> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo;
    private readonly IRepository<Attachment, Guid> TepDinhKem;

    public PhanKhaiKinhPhiGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
    }

    public async Task<PaginatedList<PhanKhaiKinhPhiDto>> Handle(PhanKhaiKinhPhiGetDanhSachQuery request, CancellationToken cancellationToken = default) {
        var search = request.SearchDto;

        var queryable = _repo.GetQueryableSet().AsNoTracking()
            .Include(e => e.TrangThai)
            .Include(e => e.NguonVon)
            .Include(e => e.DuAn)
            .Where(e => e.DuAn != null && !e.DuAn.IsDeleted)
            .WhereIf(search.DuAnId != null, e => e.DuAnId == search.DuAnId)
            .WhereIf(search.TrangThaiId > 0, e => e.TrangThaiId == search.TrangThaiId)
            .WhereIf(search.TenDuAn.IsNotNullOrWhitespace(),
                e => e.DuAn!.TenDuAn!.ToLower().Contains(search.TenDuAn!.ToLower()))
            .WhereFunc(search.DonViPhuTrachChinhId.HasValue, q => q
                .WhereIf(search.DonViPhuTrachChinhId > 0,
                    e => e.DuAn!.DonViPhuTrachChinhId == search.DonViPhuTrachChinhId)
                .WhereIf(search.DonViPhuTrachChinhId == -1,
                    e => e.DuAn!.DonViPhuTrachChinhId == null))
            .WhereIf(search.LoaiDuAnTheoNamId > 0,
                e => e.DuAn!.LoaiDuAnTheoNamId == search.LoaiDuAnTheoNamId)
            .WhereGlobalFilter(search,
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
                MaTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(search.Skip(), search.Take(), cancellationToken: cancellationToken);
    }
}
