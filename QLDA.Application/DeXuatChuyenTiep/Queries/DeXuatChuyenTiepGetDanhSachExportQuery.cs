using Microsoft.EntityFrameworkCore;
using QLDA.Application.DeXuatChuyenTieps.DTOs;

namespace QLDA.Application.DeXuatChuyenTieps.Queries;

public record DeXuatChuyenTiepGetDanhSachExportQuery : IRequest<List<DeXuatChuyenTiepExportDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
}

internal class DeXuatChuyenTiepGetDanhSachExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DeXuatChuyenTiepGetDanhSachExportQuery, List<DeXuatChuyenTiepExportDto>> {
    private readonly IRepository<DeXuatChuyenTiep, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();

    public async Task<List<DeXuatChuyenTiepExportDto>> Handle(
        DeXuatChuyenTiepGetDanhSachExportQuery request,
        CancellationToken cancellationToken = default) {
        var rows = await _repo.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .OrderBy(e => e.Index)
            .ThenBy(e => e.CreatedAt)
            .Select(e => new {
                e.SoLieuGiaiNgan,
                e.UocGiaiNgan,
                e.NhuCauKinhPhi,
                e.KhoiLuongThucTe,
                e.KhoiLuongDuKien,
            })
            .ToListAsync(cancellationToken);

        return rows.Select((row, index) => new DeXuatChuyenTiepExportDto {
            Stt = index + 1,
            SoLieuGiaiNgan = row.SoLieuGiaiNgan,
            UocGiaiNgan = row.UocGiaiNgan,
            NhuCauKinhPhi = row.NhuCauKinhPhi,
            KhoiLuongThucTe = row.KhoiLuongThucTe,
            KhoiLuongDuKien = row.KhoiLuongDuKien,
        }).ToList();
    }
}
