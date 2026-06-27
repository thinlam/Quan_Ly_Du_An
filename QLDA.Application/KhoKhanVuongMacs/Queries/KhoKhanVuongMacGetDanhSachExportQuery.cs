using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.KhoKhanVuongMacs.DTOs;

namespace QLDA.Application.KhoKhanVuongMacs.Queries;

public record KhoKhanVuongMacGetDanhSachExportQuery
    : IMayHaveGlobalFilter, IFromDateToDate, IRequest<List<KhoKhanVuongMacExportDto>>
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public string? NoiDung { get; set; }
    public int? TinhTrangId { get; set; }
    public int? MucDoKhoKhanId { get; set; }
    public int? LoaiDuAnId { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public long? LanhDaoPhuTrachId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public long? DonViPhoiHopId { get; set; }
}

internal class KhoKhanVuongMacGetDanhSachExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KhoKhanVuongMacGetDanhSachExportQuery, List<KhoKhanVuongMacExportDto>>
{
    private readonly IRepository<BaoCaoKhoKhanVuongMac, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<BaoCaoKhoKhanVuongMac, Guid>>();
    private readonly IAuthorizationManager _authManager =
        serviceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<List<KhoKhanVuongMacExportDto>> Handle(
        KhoKhanVuongMacGetDanhSachExportQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _authManager.FilterVisible(_repo.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
            .AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.TinhTrangId > 0, e => e.TinhTrangId == request.TinhTrangId)
            .WhereIf(request.MucDoKhoKhanId > 0, e => e.MucDoKhoKhanId == request.MucDoKhoKhanId)
            .WhereIf(request.LoaiDuAnId > 0, e => e.DuAn!.LoaiDuAnId == request.LoaiDuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.NoiDung.IsNotNullOrWhitespace(),
                e => e.NoiDung!.ToLower().Contains(request.NoiDung!.ToLower()))
            .WhereIf(request.TuNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(request, e => e.NoiDung, e => e.TinhTrang!.Ten);

        var rows = await queryable
            .OrderByDescending(e => e.Ngay)
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                e.Ngay,
                e.NoiDung,
                e.TinhTrangId,
                e.MucDoKhoKhanId,
                TenDuAn = e.DuAn!.TenDuAn,
                TenBuoc = e.DuAnBuoc!.TenBuoc ?? e.DuAnBuoc.Buoc!.Ten,
            })
            .ToListAsync(cancellationToken);

        ManagedException.ThrowIf(rows.Count == 0, "Không có dữ liệu để xuất");

        return rows.Select((row, index) => new KhoKhanVuongMacExportDto
        {
            Stt = index + 1,
            TenDuAn = row.TenDuAn,
            TenBuoc = row.TenBuoc,
            Ngay = row.Ngay,
            TinhTrangId = row.TinhTrangId,
            NoiDung = row.NoiDung,
            MucDoKhoKhanId = row.MucDoKhoKhanId,
        }).ToList();
    }
}
