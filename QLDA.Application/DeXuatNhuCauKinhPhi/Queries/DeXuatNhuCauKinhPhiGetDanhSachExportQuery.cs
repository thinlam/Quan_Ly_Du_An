using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Queries;

public record DeXuatNhuCauKinhPhiGetDanhSachExportQuery : IMayHaveGlobalFilter, IRequest<List<DeXuatNhuCauKinhPhiExportDto>> {
    public Guid? DuAnId { get; set; }
    public int? TrangThaiId { get; set; }
    public string? GlobalFilter { get; set; }
}

internal class DeXuatNhuCauKinhPhiGetDanhSachExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DeXuatNhuCauKinhPhiGetDanhSachExportQuery, List<DeXuatNhuCauKinhPhiExportDto>> {
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
    private readonly IRepository<DmDonVi, long> _dmDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    public async Task<List<DeXuatNhuCauKinhPhiExportDto>> Handle(
        DeXuatNhuCauKinhPhiGetDanhSachExportQuery request,
        CancellationToken cancellationToken = default) {
        var dmDonViQuery = _dmDonVi.GetQueryableSet().AsNoTracking();

        var queryable = _repo.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.TrangThaiId != null, e => e.TrangThaiId == request.TrangThaiId);

        if (!string.IsNullOrWhiteSpace(request.GlobalFilter)) {
            var filterValue = request.GlobalFilter.Trim().ToLower(CultureInfo.CurrentCulture);
            queryable = queryable.Where(e =>
                (e.TrichYeu != null && e.TrichYeu.ToLower().Contains(filterValue))
                || (e.SoPhieuChuyen != null && e.SoPhieuChuyen.ToLower().Contains(filterValue))
                || dmDonViQuery.Any(dv => e.DonViDeXuatId != null
                    && dv.Id == e.DonViDeXuatId
                    && dv.TenDonVi != null
                    && dv.TenDonVi.ToLower().Contains(filterValue)));
        }

        var rows = await queryable
            .OrderBy(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .Select(e => new {
                e.TrichYeu,
                e.KinhPhiDeXuat,
                e.SoPhieuChuyen,
                TenPhongDeXuat = dmDonViQuery
                    .Where(dv => dv.Id == e.DonViDeXuatId)
                    .Select(dv => dv.TenDonVi)
                    .FirstOrDefault(),
                TenTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG"
                    ? e.TrangThai!.Ten
                    : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .ToListAsync(cancellationToken);

        return rows.Select((row, index) => new DeXuatNhuCauKinhPhiExportDto {
            Stt = index + 1,
            TrichYeu = row.TrichYeu,
            KinhPhiDeXuat = row.KinhPhiDeXuat,
            TenPhongDeXuat = row.TenPhongDeXuat,
            SoPhieuChuyen = row.SoPhieuChuyen,
            TenTrangThai = row.TenTrangThai,
        }).ToList();
    }
}
