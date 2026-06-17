using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

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
    public string? TrangThai { get; set; }
    
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class PheDuyetGetDanhSachQueryHandler : IRequestHandler<PheDuyetGetDanhSachQuery, PaginatedList<PheDuyetListItemDto>> {
    private readonly IRepository<PheDuyetDuToan, Guid> _duToanRepo;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepo;
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> _hoSoCnttRepo;
    private readonly IRepository<HoSoMoiThauDienTu, Guid> _hoSoThauRepo;
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _baoCaoKhaoSatRepo;
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _quyetDinhDieuChinhRepo;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepo;
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationContext _authContext;

    public PheDuyetGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _duToanRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _pheDuyetRepo = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _hoSoCnttRepo = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        _hoSoThauRepo = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _baoCaoKhaoSatRepo = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _quyetDinhDieuChinhRepo = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _historyRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PaginatedList<PheDuyetListItemDto>> Handle(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken) {
        var validTypes = new[] {
            PheDuyetEntityNames.PheDuyetDuToan,
            PheDuyetEntityNames.HoSoDeXuatCapDoCntt,
            PheDuyetEntityNames.HoSoMoiThauDienTu,
            PheDuyetEntityNames.BaoCaoKetQuaKhaoSat,
            PheDuyetEntityNames.DeXuatChuTruongMoi,
            PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
            PheDuyetEntityNames.DeXuatNhuCauKinhPhi,
            PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam,
            PheDuyetEntityNames.ThuyetMinhDuAn,
            PheDuyetEntityNames.ToTrinhKeHoach,
            PheDuyetEntityNames.ToTrinhKetQuaGoiThau,
            PheDuyetEntityNames.ToTrinhThamDinhNhaThau,
            PheDuyetEntityNames.QuyetDinhKeHoachThue,
            PheDuyetEntityNames.DuToanDauTu,
            PheDuyetEntityNames.KHLCNTDuToanYeuCauRieng,
            PheDuyetEntityNames.KHLCNTDuToanSanCo,
            PheDuyetEntityNames.QuyetDinhDieuChinh,
            PheDuyetEntityNames.KeHoachTrienKhaiHangMuc,
        };
        if (request.Type != null && !validTypes.Contains(request.Type)) {
            return new PaginatedList<PheDuyetListItemDto>([], 0, request.Skip(), request.Take());
        }

        var items = new List<PheDuyetListItemDto>();

        if (request.Type == PheDuyetEntityNames.PheDuyetDuToan)
        {
            items.AddRange(await GetDuToanItems(request, cancellationToken));
        }

        if (request.Type == PheDuyetEntityNames.HoSoDeXuatCapDoCntt)
        {
            items.AddRange(await GetHoSoDeXuatCapDoCnttItems(request, cancellationToken));
        }

        if (request.Type == PheDuyetEntityNames.HoSoMoiThauDienTu)
        {
            items.AddRange(await GetHoSoMoiThauDienTuItems(request, cancellationToken));
        }

        if ( request.Type == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat)
        {
            items.AddRange(await GetBaoCaoKetQuaKhaoSatItems(request, cancellationToken));
        }
        else
        {
            items.AddRange(await GetPheDuyetAll(request, cancellationToken));
        }
        // chỉ lấy từ pheDuyetHistory


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
            .WhereIf(request.TrangThai != null, e => e.TrangThai.Ma == request.TrangThai)
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
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
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
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
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
                MaTrangThai = e.TrangThaiPheDuyet != null && e.TrangThaiPheDuyet.Ma != "LEG" ? e.TrangThaiPheDuyet.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThaiPheDuyet != null && e.TrangThaiPheDuyet.Ma != "LEG" ? e.TrangThaiPheDuyet.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            });

        var items = await query.ToListAsync(cancellationToken);
        foreach (var item in items)
            item.NgayXuLyMoiNhat = latestDates.GetValueOrDefault(item.Id);

        return items;
    }
    private async Task<List<PheDuyetListItemDto>> GetPheDuyetAll(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken)
    {
        var historyData = await _historyRepo.GetQueryableSet()
            .Where(h => h.EntityName == request.Type)
            
            .Select(h => new { h.EntityId, h.NgayXuLy })
            .ToListAsync(cancellationToken);

        //var latestDates = historyData
        //    .GroupBy(h => h.EntityId)
        //    .ToDictionary(g => g.Key, g => g.Max(x => x.NgayXuLy));
        var duAnBuoc = _duAnBuocRepo.GetQueryableSet().AsNoTracking();
        var pheDuyetQuery = _buocAuth.FilterVisibleChildEntities(_pheDuyetRepo.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId);

        var query = from e in pheDuyetQuery
            join b in duAnBuoc   on e.BuocId equals b.Id into buocGroup
            from b in buocGroup.DefaultIfEmpty()
            where !e.IsDeleted
            select new PheDuyetListItemDto
            {
                Id = e.Id,
                Type = request.Type,
                DuAnId = e.DuAnId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,
                TenBuoc = b != null ? b.TenBuoc : null,
                TrichYeu = e.NoiDung,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG"  ? e.TrangThai.Ma   : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten  : TrangThaiPheDuyetCodes.Default.TenDuThao,
                NgayXuLyMoiNhat = e.UpdatedAt
            };
        var items = await query.ToListAsync(cancellationToken);
        //foreach (var item in items)
        //    item.NgayXuLyMoiNhat = latestDates.GetValueOrDefault(item.Id);

        return items;
    }
    private async Task<List<PheDuyetListItemDto>> GetBaoCaoKetQuaKhaoSatItems(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken) {
        var historyData = await _historyRepo.GetQueryableSet()
            .Where(h => h.EntityName == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat)
            .Select(h => new { h.EntityId, h.NgayXuLy })
            .ToListAsync(cancellationToken);

        var latestDates = historyData
            .GroupBy(h => h.EntityId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.NgayXuLy));

        var query = _baoCaoKhaoSatRepo.GetQueryableSet().AsNoTracking()
            .Include(e => e.TrangThai)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereGlobalFilter(request, e => e.NoiDungBaoCao, e => e.NoiDungNghiemThu)
            .Select(e => new PheDuyetListItemDto {
                Id = e.Id,
                Type = PheDuyetEntityNames.BaoCaoKetQuaKhaoSat,
                DuAnId = e.DuAnId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,
                TrichYeu = e.NoiDungBaoCao,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            });

        var items = await query.ToListAsync(cancellationToken);
        foreach (var item in items)
            item.NgayXuLyMoiNhat = latestDates.GetValueOrDefault(item.Id);

        return items;
    }
}
