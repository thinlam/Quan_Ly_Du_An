using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.PhanKhaiKinhPhis.Queries;

public record PhanKhaiKinhPhiGetDanhSachExportQuery : IMayHaveGlobalFilter,
    IRequest<List<PhanKhaiKinhPhiDanhSachExportDto>> {
    public Guid? DuAnId { get; set; }
    public Guid? Id { get; set; }
    public string? GlobalFilter { get; set; }
    public int? TrangThaiId { get; set; }
}

internal class PhanKhaiKinhPhiGetDanhSachExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<PhanKhaiKinhPhiGetDanhSachExportQuery, List<PhanKhaiKinhPhiDanhSachExportDto>> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();

    public async Task<List<PhanKhaiKinhPhiDanhSachExportDto>> Handle(
        PhanKhaiKinhPhiGetDanhSachExportQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = _repo.GetQueryableSet().AsNoTracking()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn)
            .Include(e => e.NguonVon)
            .WhereIf(request.Id != null, e => e.Id == request.Id)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.TrangThaiId > 0, e => e.TrangThaiId == request.TrangThaiId)
            .WhereGlobalFilter(request,
                e => e.SoToTrinh,
                e => e.NguonVon != null ? e.NguonVon.Ten : null
            );

        var rows = await queryable
            .OrderBy(e => e.SoToTrinh)
            .ToListAsync(cancellationToken);
        try
        {
            return rows.Select((e, index) => new PhanKhaiKinhPhiDanhSachExportDto
            {
                Stt = index + 1,
                TenDuAn = e.DuAn?.TenDuAn,
                KinhPhiDeXuat = e.KinhPhiDeXuat,
                KinhPhiPhanKhai = e.KinhPhiPhanKhai,
                TongMucDauTu = e.DuAn?.TongMucDauTu,
                SoToTrinh = e.SoToTrinh,
                NgayToTrinh = e.NgayToTrinh,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG"
              ? e.TrangThai.Ten
              : TrangThaiPheDuyetCodes.Default.TenDuThao,
            }).ToList();
        }
        catch (Exception ex)
        {

            throw;
        }
      
    }
}
