using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.TongHopDeXuatChuTruongs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.TongHopDeXuatChuTruongs.Queries;

public record TongHopDeXuatNhuCauKinhPhiGetExportQuery : IMayHaveGlobalFilter,
    IRequest<List<TongHopDeXuatNhuCauKinhPhiExportDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public string? Loai { get; set; }
    public int? Nam { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public long? DonViPhuTrachId { get; set; }
}

internal class TongHopDeXuatNhuCauKinhPhiGetExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<TongHopDeXuatNhuCauKinhPhiGetExportQuery, List<TongHopDeXuatNhuCauKinhPhiExportDto>> {
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> _deXuatNhuCauKinhPhi =
        serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
    private readonly IRepository<DeXuatChuTruongMoi, Guid> _deXuatChuTruongMoi =
        serviceProvider.GetRequiredService<IRepository<DeXuatChuTruongMoi, Guid>>();
    private readonly IRepository<DeXuatChuyenTiep, Guid> _deXuatChuyenTiep =
        serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();
    private readonly IRepository<DmDonVi, long> _dmDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    public async Task<List<TongHopDeXuatNhuCauKinhPhiExportDto>> Handle(
        TongHopDeXuatNhuCauKinhPhiGetExportQuery request,
        CancellationToken cancellationToken = default) {
        var dmDonViQuery = _dmDonVi.GetQueryableSet().AsNoTracking();

        var duAnMoiIds = BuildDuAnMoiIdsQuery(request);
        var duAnChuyenTiepIds = BuildDuAnChuyenTiepIdsQuery(request);

        var queryable = _deXuatNhuCauKinhPhi.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId);

        queryable = request.Loai switch {
            "DeXuatMoi" => queryable.Where(e => duAnMoiIds.Contains(e.DuAnId)),
            "ChuyenTiep" => queryable.Where(e => duAnChuyenTiepIds.Contains(e.DuAnId)),
            _ => queryable.Where(e => duAnMoiIds.Contains(e.DuAnId) || duAnChuyenTiepIds.Contains(e.DuAnId)),
        };

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

        return rows.Select((row, index) => new TongHopDeXuatNhuCauKinhPhiExportDto {
            Stt = index + 1,
            TrichYeu = row.TrichYeu,
            KinhPhiDeXuat = row.KinhPhiDeXuat,
            TenPhongDeXuat = row.TenPhongDeXuat,
            SoPhieuChuyen = row.SoPhieuChuyen,
            TenTrangThai = row.TenTrangThai,
        }).ToList();
    }

    private IQueryable<Guid> BuildDuAnMoiIdsQuery(TongHopDeXuatNhuCauKinhPhiGetExportQuery request) =>
        _deXuatChuTruongMoi.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.DonViPhuTrachId != null, e => e.DonViPhuTrachChinhId == request.DonViPhuTrachId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.Nam != null, e => e.NamDeXuat == request.Nam)
            .Select(e => e.DuAnId);

    private IQueryable<Guid> BuildDuAnChuyenTiepIdsQuery(TongHopDeXuatNhuCauKinhPhiGetExportQuery request) =>
        _deXuatChuyenTiep.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.Nam != null, e => e.NamDeXuat == request.Nam)
            .Select(e => e.DuAnId);
}
