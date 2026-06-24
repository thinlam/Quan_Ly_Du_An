using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Queries;

public record KeHoachTrienKhaiHangMucGetExportQuery : IFromDateToDate, IMayHaveGlobalFilter,
    IRequest<List<KeHoachTrienKhaiHangMucExportItemDto>>
{
    public Guid? Id { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? GlobalFilter { get; set; }
    public int? TrangThaiId { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class KeHoachTrienKhaiHangMucGetExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KeHoachTrienKhaiHangMucGetExportQuery, List<KeHoachTrienKhaiHangMucExportItemDto>>
{
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> _keHoachRepo =
        serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
    private readonly IRepository<DanhMucGiaiDoan, int> _giaiDoanRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucGiaiDoan, int>>();
    private readonly IRepository<DmDonVi, long> _donViRepo =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<UserMaster, long> _userRepo =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth =
        serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext =
        serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<List<KeHoachTrienKhaiHangMucExportItemDto>> Handle(
        KeHoachTrienKhaiHangMucGetExportQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = BuildFilteredQueryable(request);
        var hangMucs = await LoadHangMucsAsync(queryable, request, cancellationToken);

        ManagedException.ThrowIf(hangMucs.Count == 0, "Không có dữ liệu để xuất");

        return await MapToExportRowsAsync(hangMucs, cancellationToken);
    }

    private IQueryable<KeHoachTrienKhaiHangMuc> BuildFilteredQueryable(
        KeHoachTrienKhaiHangMucGetExportQuery request)
    {
        DateTimeOffset? tuNgayDto = null;
        DateTimeOffset? denNgayExclusiveDto = null;
        if (request.TuNgay.HasValue)
            tuNgayDto = new DateTimeOffset(request.TuNgay.Value.ToDateTime(TimeOnly.MinValue));
        if (request.DenNgay.HasValue)
            denNgayExclusiveDto = new DateTimeOffset(request.DenNgay.Value.ToDateTime(TimeOnly.MinValue)).AddDays(1);

        return _buocAuth.FilterVisibleChildEntities(
                _keHoachRepo.GetQueryableSet(),
                _duAnBuocRepo,
                _authContext,
                e => e.BuocId)
            .AsNoTracking()
            .Include(e => e.DanhSachHangMuc)
            .WhereIf(request.Id.HasValue && request.Id != Guid.Empty, e => e.Id == request.Id)
            .WhereIf(request.DuAnId.HasValue && request.DuAnId != Guid.Empty, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.BuocId != null, e => e.BuocId == request.BuocId)
            .WhereIf(request.So != null, e => e.So.Contains(request.So!))
            .WhereIf(request.TrichYeu.IsNotNullOrWhitespace(), e => e.TrichYeu!.Contains(request.TrichYeu!))
            .WhereIf(request.TrangThaiId != null, e => e.TrangThaiId == request.TrangThaiId)
            .WhereIf(tuNgayDto != null, e => e.NgayToTrinh >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayToTrinh < denNgayExclusiveDto);
    }

    private static async Task<List<HangMucKeHoach>> LoadHangMucsAsync(
        IQueryable<KeHoachTrienKhaiHangMuc> queryable,
        KeHoachTrienKhaiHangMucGetExportQuery request,
        CancellationToken cancellationToken)
    {
        var hasId = request.Id is Guid id && id != Guid.Empty;
        var hasDuAnId = request.DuAnId is Guid duAnId && duAnId != Guid.Empty;

        if (hasId)
        {
            var keHoach = await queryable.FirstOrDefaultAsync(cancellationToken);
            return keHoach?.DanhSachHangMuc?.ToList() ?? [];
        }

        if (hasDuAnId)
        {
            var keHoach = await queryable
                .OrderByDescending(e => e.NgayToTrinh)
                .ThenByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            return keHoach?.DanhSachHangMuc?.ToList() ?? [];
        }

        var keHoachs = await queryable
            .OrderBy(e => e.So)
            .ThenByDescending(e => e.NgayToTrinh)
            .ToListAsync(cancellationToken);

        return keHoachs
            .SelectMany(k => k.DanhSachHangMuc ?? [])
            .ToList();
    }

    private async Task<List<KeHoachTrienKhaiHangMucExportItemDto>> MapToExportRowsAsync(
        List<HangMucKeHoach> hangMucs,
        CancellationToken cancellationToken)
    {
        var giaiDoanIds = hangMucs
            .Where(h => h.GiaiDoanId.HasValue)
            .Select(h => h.GiaiDoanId!.Value)
            .Distinct()
            .ToList();

        var giaiDoans = giaiDoanIds.Count == 0
            ? []
            : await _giaiDoanRepo.GetQueryableSet()
                .AsNoTracking()
                .Where(g => giaiDoanIds.Contains(g.Id))
                .ToListAsync(cancellationToken);

        var donViIds = hangMucs
            .SelectMany(h => Enumerable.Empty<long?>()
                .Append(h.DonViChuTriId)
                .Concat((h.DonViPhoiHopIds ?? []).Select(id => (long?)id)))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var donVis = donViIds.Count == 0
            ? []
            : await _donViRepo.GetQueryableSet()
                .AsNoTracking()
                .Where(d => donViIds.Contains(d.Id))
                .Select(d => new { d.Id, d.TenDonVi })
                .ToListAsync(cancellationToken);

        var userIds = hangMucs
            .SelectMany(h => Enumerable.Empty<long?>()
                .Append(h.CanBoChuTriId)
                .Concat((h.CanBoPhoiHopIds ?? []).Select(id => (long?)id)))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var users = userIds.Count == 0
            ? []
            : await _userRepo.GetQueryableSet()
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.HoTen })
                .ToListAsync(cancellationToken);

        return KeHoachTrienKhaiHangMucExportMapper.ToExportRows(
            hangMucs,
            giaiDoans.ToDictionary(g => g.Id, g => g.Ten ?? string.Empty),
            giaiDoans.ToDictionary(g => g.Id, g => g.Stt ?? int.MaxValue - 1),
            donVis.ToDictionary(d => d.Id, d => d.TenDonVi ?? string.Empty),
            users.ToDictionary(u => u.Id, u => u.HoTen ?? string.Empty));
    }
}
