using System.Globalization;
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Queries;

public record TheoDoiDeXuatNhuCauKinhPhiGetExportQuery : IMayHaveGlobalFilter, IFromDateToDate, IRequest<List<TinhHinhDeXuatNhuCauExportDto>> {
    public Guid? DuAnId { get; set; }
    public int? TrangThaiId { get; set; }
    public int? TrangThaiKeHoachId { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public string? TrichYeu { get; set; }
    public string? GlobalFilter { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public long? DonViDeXuatId { get; set; }
}

internal class TheoDoiDeXuatNhuCauKinhPhiGetExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<TheoDoiDeXuatNhuCauKinhPhiGetExportQuery, List<TinhHinhDeXuatNhuCauExportDto>> {
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> _deXuatNhuCauKinhPhi =
        serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();

    public async Task<List<TinhHinhDeXuatNhuCauExportDto>> Handle(
        TheoDoiDeXuatNhuCauKinhPhiGetExportQuery request,
        CancellationToken cancellationToken = default) {
        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.DaDuyet
                                      && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.DaTrinh
                                      && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);

        DateTimeOffset? tuNgayDto = null;
        DateTimeOffset? denNgayExclusiveDto = null;
        if (request.TuNgay.HasValue) {
            var dt = request.TuNgay.Value.ToDateTime(TimeOnly.MinValue);
            tuNgayDto = new DateTimeOffset(dt);
        }
        if (request.DenNgay.HasValue) {
            var dt = request.DenNgay.Value.ToDateTime(TimeOnly.MinValue);
            denNgayExclusiveDto = new DateTimeOffset(dt).AddDays(1);
        }

        var queryable = _deXuatNhuCauKinhPhi.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.TrangThaiKeHoachId.HasValue, e => e.DeXuatDaTrinhKeHoachNam!.Any(x =>
                x.DeXuatNhuCauKinhPhiNam != null
                && !x.DeXuatNhuCauKinhPhiNam.IsDeleted
                && x.DeXuatNhuCauKinhPhiNam.TrangThaiId == request.TrangThaiKeHoachId))
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.SoPhieuChuyen != null, e => e.SoPhieuChuyen.Contains(request.SoPhieuChuyen))
            .WhereIf(request.TrangThaiId != null, e => e.TrangThaiId == request.TrangThaiId)
            .WhereIf(tuNgayDto != null, e => e.NgayPhieuChuyen >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayPhieuChuyen < denNgayExclusiveDto)
            .WhereIf(request.DonViDeXuatId != null, x => x.DonViDeXuatId == request.DonViDeXuatId)
            .WhereIf(request.TrichYeu != null, x => x.TrichYeu == request.TrichYeu);

        var rows = await queryable
            .OrderBy(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .Select(x => new {
                x.SoPhieuChuyen,
                x.NgayPhieuChuyen,
                TenTrangThai = x.TrangThai != null ? x.TrangThai.Ten : "---",
                TenTrangThaiKeHoachNam = x.DeXuatDaTrinhKeHoachNam!
                    .Any(t =>
                        t.DeXuatNhuCauKinhPhiNam != null && !t.DeXuatNhuCauKinhPhiNam.IsDeleted
                        && t.DeXuatNhuCauKinhPhiNam.TrangThaiId == trangThaiDaTrinh!.Id)
                    ? trangThaiDaTrinh!.Ten
                    : "--",
                NgayDuyetKeHoach = x.DeXuatDaTrinhKeHoachNam!
                    .Where(t =>
                        t.DeXuatNhuCauKinhPhiNam != null && !t.DeXuatNhuCauKinhPhiNam.IsDeleted
                        && t.DeXuatNhuCauKinhPhiNam.NgayDuyet != null)
                    .Select(t => t.DeXuatNhuCauKinhPhiNam!.NgayDuyet)
                    .FirstOrDefault(),
                TenTrangThaiBanGiamDoc = x.DeXuatDaTrinhKeHoachNam!
                    .Where(t => t.DeXuatNhuCauKinhPhiNam != null && !t.DeXuatNhuCauKinhPhiNam.IsDeleted)
                    .Select(t => t.DeXuatNhuCauKinhPhiNam!.TrangThaiId == trangThaiDaDuyet!.Id
                        ? (t.DeXuatNhuCauKinhPhiNam!.TrangThai != null ? t.DeXuatNhuCauKinhPhiNam.TrangThai!.Ten : "--")
                        : "--")
                    .FirstOrDefault() ?? "--",
            })
            .ToListAsync(cancellationToken);

        return rows.Select((row, index) => new TinhHinhDeXuatNhuCauExportDto {
            Stt = index + 1,
            SoPhieuChuyen = row.SoPhieuChuyen,
            PhongPctTrinh = JoinLines(
                IsPlaceholder(row.TenTrangThai) ? null : row.TenTrangThai,
                FormatDate(row.NgayPhieuChuyen),
                row.SoPhieuChuyen),
            PhongKhtcTongHop = JoinLines(
                IsPlaceholder(row.TenTrangThaiKeHoachNam) ? null : row.TenTrangThaiKeHoachNam,
                FormatDate(row.NgayDuyetKeHoach)),
            PhongBgdPheDuyet = JoinLines(
                IsPlaceholder(row.TenTrangThaiBanGiamDoc) ? null : row.TenTrangThaiBanGiamDoc),
        }).ToList();
    }

    private static bool IsPlaceholder(string? value) =>
        string.IsNullOrWhiteSpace(value) || value is "--" or "---";

    private static string? FormatDate(DateTimeOffset? value) =>
        value?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    private static string? JoinLines(params string?[] parts) {
        var lines = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        return lines.Count == 0 ? null : string.Join("\n", lines);
    }
}
