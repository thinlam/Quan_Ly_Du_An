using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.PhanKhaiKinhPhis.Queries;

public record PhanKhaiKinhPhiGetDanhSachDaDuyetExportQuery : IMayHaveGlobalFilter, IRequest<List<PhanKhaiKinhPhiExportDto>> {
    public Guid? DuAnId { get; set; }
    public string? GlobalFilter { get; set; }
}

internal class PhanKhaiKinhPhiGetDanhSachDaDuyetExportQueryHandler
    : IRequestHandler<PhanKhaiKinhPhiGetDanhSachDaDuyetExportQuery, List<PhanKhaiKinhPhiExportDto>> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo;

    public PhanKhaiKinhPhiGetDanhSachDaDuyetExportQueryHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
    }

    public async Task<List<PhanKhaiKinhPhiExportDto>> Handle(
        PhanKhaiKinhPhiGetDanhSachDaDuyetExportQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = _repo.GetQueryableSet().AsNoTracking()
            .Include(e => e.TrangThai)
            .Include(e => e.NguonVon)
            .Include(e => e.DuAn)
            .Where(e => e.TrangThai != null
                        && e.TrangThai!.Ma == TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DaDuyet
                        && e.TrangThai.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereGlobalFilter(request,
                e => e.SoToTrinh,
                e => e.NguonVon != null ? e.NguonVon.Ten : null
            );

        var rows = await queryable
            .OrderBy(e => e.SoToTrinh)
            .Select(e => new {
                e.SoToTrinh,
                //TrichYeu = e.ThuyetMinh ?? (e.DuAn != null ? e.DuAn.Ten : null),
                TrichYeu = e.ThuyetMinh ?? (e.DuAn != null ? e.DuAn!.TenDuAn : null),
                TenNguonVon = e.NguonVon != null ? e.NguonVon.Ten : null,
                e.KinhPhiDeXuat,
                e.KinhPhiPhanKhai,
                TenTrangThai = e.TrangThai != null ? e.TrangThai!.Ten : TrangThaiPheDuyetCodes.Default.TenDaDuyet,
            })
            .ToListAsync(cancellationToken);

        return rows.Select((row, index) => new PhanKhaiKinhPhiExportDto {
            Stt = index + 1,
            SoToTrinh = row.SoToTrinh,
            TrichYeu = row.TrichYeu,
            TenNguonVon = row.TenNguonVon,
            KinhPhiDeXuat = row.KinhPhiDeXuat / 1_000_000m,
            KinhPhiPhanKhai = row.KinhPhiPhanKhai / 1_000_000m,
            TenTrangThai = row.TenTrangThai,
        }).ToList();
    }
}
