using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.Queries;

public record DeXuatNhuCauKinhPhiNamGetExportQuery : IMayHaveGlobalFilter, IFromDateToDate,
    IRequest<List<TongHopNhuCauKinhPhiNamExportDto>> {
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? TrangThaiId { get; set; }
    public string? GlobalFilter { get; set; }
}

internal class DeXuatNhuCauKinhPhiNamGetExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DeXuatNhuCauKinhPhiNamGetExportQuery, List<TongHopNhuCauKinhPhiNamExportDto>> {
    private readonly IRepository<DeXuatNhuCauKinhPhiNam, Guid> _deXuatNhuCauKinhPhiNam =
        serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhiNam, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> _tepDinhKem =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();

    public async Task<List<TongHopNhuCauKinhPhiNamExportDto>> Handle(
        DeXuatNhuCauKinhPhiNamGetExportQuery request,
        CancellationToken cancellationToken = default) {
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

        var queryable = _deXuatNhuCauKinhPhiNam.GetQueryableSet().AsNoTracking()
            .WhereIf(request.So != null, e => e.So.Contains(request.So))
            .WhereIf(request.TrichYeu != null, e => e.So.Contains(request.TrichYeu))
            .WhereIf(request.TrangThaiId != null, e => e.TrangThaiId == request.TrangThaiId)
            .WhereIf(tuNgayDto != null, e => e.NgayKeHoach >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayKeHoach < denNgayExclusiveDto);

        var rows = await queryable
            .OrderBy(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .Select(e => new {
                e.So,
                e.TrichYeu,
                e.TongKinhPhiDeXuat,
                e.NgayKeHoach,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG"
                    ? e.TrangThai.Ten
                    : string.Empty,
                SoLuongTepDinhKem = _tepDinhKem.GetQueryableSet()
                    .Count(i => i.GroupId == e.Id.ToString()),
            })
            .ToListAsync(cancellationToken);

        return rows.Select((row, index) => new TongHopNhuCauKinhPhiNamExportDto {
            Stt = index + 1,
            SoKeHoach = row.So,
            TrichYeu = row.TrichYeu,
            TongHopChiPhi = row.TongKinhPhiDeXuat,
            Ngay = row.NgayKeHoach?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            TrangThai = row.TenTrangThai,
            SoLuongTepDinhKem = row.SoLuongTepDinhKem,
        }).ToList();
    }
}
