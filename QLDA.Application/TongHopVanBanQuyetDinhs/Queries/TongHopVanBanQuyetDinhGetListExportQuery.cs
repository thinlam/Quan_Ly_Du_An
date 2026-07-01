using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TongHopVanBanQuyetDinhs.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.TongHopVanBanQuyetDinhs.Queries;

public record TongHopVanBanQuyetDinhGetListExportQuery
    : IMayHaveGlobalFilter, IFromDateToDate,
      IRequest<List<TongHopVanBanQuyetDinhExportDto>>
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public EnumLoaiVanBanQuyetDinh? Loai { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
}

internal class TongHopVanBanQuyetDinhGetListExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<TongHopVanBanQuyetDinhGetListExportQuery, List<TongHopVanBanQuyetDinhExportDto>>
{
    private readonly IRepository<VanBanQuyetDinh, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
    private readonly IAuthorizationManager _authManager =
        serviceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<List<TongHopVanBanQuyetDinhExportDto>> Handle(
        TongHopVanBanQuyetDinhGetListExportQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _authManager.FilterVisible(_repo.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
            .AsNoTracking()
            .WhereIf(request.Loai.HasValue, e => e.Loai == request.Loai.ToString())
            .WhereIf(request.DuAnId.HasValue, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(!string.IsNullOrEmpty(request.CoQuanQuyetDinh),
                e => e.CoQuanQuyetDinh!.Contains(request.CoQuanQuyetDinh))
            .WhereIf(request.TrichYeu.IsNotNullOrWhitespace(),
                e => e.TrichYeu!.ToLower().Contains(request.TrichYeu!.ToLower()))
            .WhereIf(request.TuNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(request, e => e.So, e => e.TrichYeu, e => e.DuAn!.TenDuAn);

        var rows = await queryable
            .OrderByDescending(e => e.Ngay ?? e.NgayKy)
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                TenDuAn = e.DuAn!.TenDuAn,
                e.So,
                Ngay = e.Ngay ?? e.NgayKy,
                LoaiRaw = e.Loai,
                e.TrichYeu,
            })
            .ToListAsync(cancellationToken);

        ManagedException.ThrowIf(rows.Count == 0, "Không có dữ liệu để xuất");

        return rows.Select((row, index) => new TongHopVanBanQuyetDinhExportDto
        {
            Stt = index + 1,
            TenDuAn = row.TenDuAn,
            So = row.So,
            Ngay = row.Ngay,
            TrichYeu = row.TrichYeu,
            Loai = row.LoaiRaw!.GetDescriptionFromName<EnumLoaiVanBanQuyetDinh>(),
        }).ToList();
    }
}
