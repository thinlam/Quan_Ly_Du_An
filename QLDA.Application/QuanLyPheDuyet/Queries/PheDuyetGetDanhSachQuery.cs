using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Queries;

/// <summary>
/// Danh sach tong hop tat ca ban ghi pheduyet voi trang thai moi nhat tu PheDuyetHistory
/// </summary>
public record PheDuyetGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PheDuyetListItemDto>> {
    public Guid? DuAnId { get; set; }
    public string? Type { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class PheDuyetGetDanhSachQueryHandler : IRequestHandler<PheDuyetGetDanhSachQuery, PaginatedList<PheDuyetListItemDto>> {
    private readonly IRepository<PheDuyetDuToan, Guid> _duToanRepo;
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> _hoSoCnttRepo;
    private readonly IRepository<HoSoMoiThauDienTu, Guid> _hoSoThauRepo;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepo;

    public PheDuyetGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _duToanRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _hoSoCnttRepo = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        _hoSoThauRepo = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _historyRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
    }

    public async Task<PaginatedList<PheDuyetListItemDto>> Handle(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken) {
        var validTypes = new[] { PheDuyetEntityNames.PheDuyetDuToan, PheDuyetEntityNames.HoSoDeXuatCapDoCntt, PheDuyetEntityNames.HoSoMoiThauDienTu };
        if (request.Type != null && !validTypes.Contains(request.Type)) {
            return new PaginatedList<PheDuyetListItemDto>([], 0, request.Skip(), request.Take());
        }

        var items = new List<PheDuyetListItemDto>();

        if (request.Type == null || request.Type == PheDuyetEntityNames.PheDuyetDuToan) {
            items.AddRange(await GetDuToanItems(request, cancellationToken));
        }

        if (request.Type == null || request.Type == PheDuyetEntityNames.HoSoDeXuatCapDoCntt) {
            items.AddRange(await GetHoSoDeXuatCapDoCnttItems(request, cancellationToken));
        }

        if (request.Type == null || request.Type == PheDuyetEntityNames.HoSoMoiThauDienTu) {
            items.AddRange(await GetHoSoMoiThauDienTuItems(request, cancellationToken));
        }

        var sorted = items.OrderByDescending(i => i.NgayXuLyMoiNhat ?? DateTimeOffset.MinValue).ToList();
        return new PaginatedList<PheDuyetListItemDto>(sorted.Skip(request.Skip()).Take(request.Take()).ToList(), sorted.Count, request.Skip(), request.Take());
    }

    private async Task<List<PheDuyetListItemDto>> GetDuToanItems(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken) {
        var historyData = await _historyRepo.GetQueryableSet()
            .Where(h => h.EntityName == PheDuyetEntityNames.PheDuyetDuToan)
            .Select(h => new { h.EntityId, h.NgayXuLy })
            .ToListAsync(cancellationToken);

        var latestDates = historyData
            .GroupBy(h => h.EntityId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.NgayXuLy));

        var query = _duToanRepo.GetQueryableSet().AsNoTracking()
            .Include(e => e.TrangThai)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereGlobalFilter(request, e => e.So, e => e.NguoiKy, e => e.TrichYeu)
            .Select(e => new PheDuyetListItemDto {
                Id = e.Id,
                Type = PheDuyetEntityNames.PheDuyetDuToan,
                DuAnId = e.DuAnId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,
                SoVanBan = e.So,
                TrichYeu = e.TrichYeu,
                NguoiKy = e.NguoiKy,
                NgayKy = e.NgayKy,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null ? e.TrangThai.Ma : null,
                TenTrangThai = e.TrangThai != null ? e.TrangThai.Ten : null,
            });

        var items = await query.ToListAsync(cancellationToken);
        foreach (var item in items)
            item.NgayXuLyMoiNhat = latestDates.GetValueOrDefault(item.Id);

        return items;
    }

    private async Task<List<PheDuyetListItemDto>> GetHoSoDeXuatCapDoCnttItems(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken) {
        var historyData = await _historyRepo.GetQueryableSet()
            .Where(h => h.EntityName == PheDuyetEntityNames.HoSoDeXuatCapDoCntt)
            .Select(h => new { h.EntityId, h.NgayXuLy })
            .ToListAsync(cancellationToken);

        var latestDates = historyData
            .GroupBy(h => h.EntityId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.NgayXuLy));

        var query = _hoSoCnttRepo.GetQueryableSet().AsNoTracking()
            .Include(e => e.TrangThai)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereGlobalFilter(request, e => e.NoiDungDeNghi, e => e.NoiDungBaoCao, e => e.NoiDungDuThao)
            .Select(e => new PheDuyetListItemDto {
                Id = e.Id,
                Type = PheDuyetEntityNames.HoSoDeXuatCapDoCntt,
                DuAnId = e.DuAnId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,
                TrichYeu = e.NoiDungDeNghi,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null ? e.TrangThai.Ma : null,
                TenTrangThai = e.TrangThai != null ? e.TrangThai.Ten : null,
            });

        var items = await query.ToListAsync(cancellationToken);
        foreach (var item in items)
            item.NgayXuLyMoiNhat = latestDates.GetValueOrDefault(item.Id);

        return items;
    }

    private async Task<List<PheDuyetListItemDto>> GetHoSoMoiThauDienTuItems(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken) {
        var historyData = await _historyRepo.GetQueryableSet()
            .Where(h => h.EntityName == PheDuyetEntityNames.HoSoMoiThauDienTu)
            .Select(h => new { h.EntityId, h.NgayXuLy })
            .ToListAsync(cancellationToken);

        var latestDates = historyData
            .GroupBy(h => h.EntityId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.NgayXuLy));

        var query = _hoSoThauRepo.GetQueryableSet().AsNoTracking()
            .Include(e => e.TrangThaiPheDuyet)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereGlobalFilter(request, e => e.ThoiGianThucHien, e => e.ThoiGianThucHien, e => e.ThoiGianThucHien)
            .Select(e => new PheDuyetListItemDto {
                Id = e.Id,
                Type = PheDuyetEntityNames.HoSoMoiThauDienTu,
                DuAnId = e.DuAnId ?? Guid.Empty,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThaiPheDuyet != null ? e.TrangThaiPheDuyet.Ma : null,
                TenTrangThai = e.TrangThaiPheDuyet != null ? e.TrangThaiPheDuyet.Ten : null,
            });

        var items = await query.ToListAsync(cancellationToken);
        foreach (var item in items)
            item.NgayXuLyMoiNhat = latestDates.GetValueOrDefault(item.Id);

        return items;
    }
}
