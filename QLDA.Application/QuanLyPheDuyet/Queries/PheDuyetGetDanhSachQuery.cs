using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

using QLDA.Application.Common.Mapping;
using QLDA.Application.DuongDiTrangThaiToTrinhs.DTOs;
using QLDA.Application.Providers;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using UserMaster = BuildingBlocks.Domain.Entities.UserMaster;

namespace QLDA.Application.QuanLyPheDuyet.Queries;

/// <summary>
/// Danh sach tong hop tat ca ban ghi pheduyet voi trang thai moi nhat tu PheDuyetHistory
/// </summary>
public record PheDuyetGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PheDuyetListItemDto>>
{
    public Guid? DuAnId { get; set; }
    public string? Type { get; set; }
    public string? TrangThai { get; set; }

    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }

    /// <summary>False khi export Excel — không load tệp đính kèm.</summary>
    public bool IncludeAttachments { get; set; } = true;
}

internal class PheDuyetGetDanhSachQueryHandler : IRequestHandler<PheDuyetGetDanhSachQuery, PaginatedList<PheDuyetListItemDto>>
{
    private readonly IUserProvider _userProvider;
    private readonly IRepository<PheDuyetDuToan, Guid> _duToanRepo;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IRepository<DuAn, Guid> _duAnRepo;
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepo;
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> _hoSoCnttRepo;
    private readonly IRepository<HoSoMoiThauDienTu, Guid> _hoSoThauRepo;
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _baoCaoKhaoSatRepo;
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _quyetDinhDieuChinhRepo;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepo;
    private readonly IRepository<PheDuyet, Guid> _PheDuyetRepo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _DmTrangThaiPheDuyetRepo;
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationContext _authContext;
    private readonly IAppSettingsProvider _settings;
    private readonly IRepository<TepDinhKem, Guid> _tepDinhKemRepo;
    private readonly IRepository<DuongDiTrangThaiToTrinh, long> _duongDiTrangThaiToTrinh;
    private readonly IRepository<UserMaster, long> _userMasterRepo;
    public PheDuyetGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _PheDuyetRepo = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _duToanRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _duAnRepo = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _pheDuyetRepo = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _hoSoCnttRepo = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        _hoSoThauRepo = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _baoCaoKhaoSatRepo = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _quyetDinhDieuChinhRepo = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _historyRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _tepDinhKemRepo = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _DmTrangThaiPheDuyetRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _duongDiTrangThaiToTrinh = serviceProvider.GetRequiredService<IRepository<DuongDiTrangThaiToTrinh, long>>();
        _userMasterRepo = serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PaginatedList<PheDuyetListItemDto>> Handle(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken)
    {
        var validTypes = new[] {
            PheDuyetEntityNames.PheDuyetDuToan,
            PheDuyetEntityNames.HoSoDeXuatCapDoCntt,
            PheDuyetEntityNames.HoSoMoiThauDienTu,
            PheDuyetEntityNames.BaoCaoKetQuaKhaoSat,
            PheDuyetEntityNames.DeXuatChuTruongMoi,
       //     PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
        //    PheDuyetEntityNames.DeXuatNhuCauKinhPhi,
            //PheDuyetEntityNames.ThuyetMinhDuAn,
            // thiếu tờ trình phê duyet5 khảo sát

            PheDuyetEntityNames.BaoCaoKetQuaKhaoSat,//add new
            PheDuyetEntityNames.ChuTruongLapKeHoach,//addnew
            PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam,
            PheDuyetEntityNames.ToTrinhKeHoach,// cái này đang dùng chung 8 màn hình
            PheDuyetEntityNames.ToTrinhKetQuaGoiThau,
            PheDuyetEntityNames.ToTrinhThamDinhNhaThau,
            PheDuyetEntityNames.QuyetDinhKeHoachThue,
            PheDuyetEntityNames.DuToanDauTu,
            PheDuyetEntityNames.KHLCNTDuToanYeuCauRieng,
            PheDuyetEntityNames.KHLCNTDuToanSanCo,
            PheDuyetEntityNames.QuyetDinhDieuChinh,// xem lại dữ liệu
            PheDuyetEntityNames.KeHoachTrienKhaiHangMuc,
        };
        var userId = _userProvider.Info.UserID;
        // Dùng HasKhtcBypass từ IAuthorizationContext (cached per request) thay cho check trực tiếp PhongBanID.
        var duongDi = await _duongDiTrangThaiToTrinh.GetQueryableSet().AsNoTracking()
           .Where(x => x.Used && !(x.IsDeleted ?? false)).ToListAsync(cancellationToken);
        var duongDiLookup = duongDi
              .Where(x => !string.IsNullOrWhiteSpace(x.MaTrangThaiHienTai))
              .GroupBy(x => (
                  x.MaTrangThaiHienTai!.Trim(), x.Loai))
              .ToDictionary(
                  g => g.Key,
                  g => g.Select(x => new DuongDiTrangThaiToTrinhDto {
                      MaTrangThaiHienTai = x.MaTrangThaiHienTai,
                      MaTrangThaiTiepTheo = x.MaTrangThaiTiepTheo,
                      TenTrangThaiTiepTheo = x.TenTrangThaiTiepTheo,
                      RoleId = x.RoleId,
                      RoleLevel = x.RoleLevel
                  }).ToList()
              );

        var finalQuery = PheDuyetQueryableExtensions.ApplyDanhSachFilters(
            new PheDuyetDanhSachFilter(request.Type, request.TrangThai),
            _PheDuyetRepo,
            _duAnRepo,
            _duAnBuocRepo,
            request.IncludeAttachments ? _tepDinhKemRepo : null,
            _authContext,
            userId,
            includeAttachments: request.IncludeAttachments);

        // PageSize=0 / Take()=0 → lấy hết (dùng cho export Excel)
        var pagiList = PaginatedList<PheDuyetListItemDto>.Create(finalQuery, request.Skip(), request.Take());
        await ResolveUserNamesAsync(pagiList.Data, cancellationToken);
        foreach (var item in pagiList.Data) {
            item.ThaoTacTiepTheo =  !string.IsNullOrEmpty(item.MaTrangThai)
                                    && duongDiLookup.TryGetValue((
                                        item.MaTrangThai.Trim(),
                                        item.EntityName ),
                                    out var actions)
                                    ? actions : [];}
        return pagiList;
        #region old
        //var items = await GetPheDuyetAll(request, cancellationToken);


        //if (request.Type == PheDuyetEntityNames.PheDuyetDuToan)
        //{
        //    items.AddRange(await GetDuToanItems(request, cancellationToken));
        //}

        //if (request.Type == PheDuyetEntityNames.HoSoDeXuatCapDoCntt)
        //{
        //    items.AddRange(await GetHoSoDeXuatCapDoCnttItems(request, cancellationToken));
        //}

        //if (request.Type == PheDuyetEntityNames.HoSoMoiThauDienTu)
        //{
        //    items.AddRange(await GetHoSoMoiThauDienTuItems(request, cancellationToken));
        //}

        //if (request.Type == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat)
        //{
        //    items.AddRange(await GetBaoCaoKetQuaKhaoSatItems(request, cancellationToken));
        //}
        //else
        //{
        //     items.AddRange(await GetPheDuyetAll(request, cancellationToken));
        //  }
        // chỉ lấy từ pheDuyetHistory


        // var sorted = items.OrderByDescending(i => i.NgayXuLyMoiNhat ?? DateTimeOffset.MinValue).ToList();
        // return new PaginatedList<PheDuyetListItemDto>(sorted.Skip(request.Skip()).Take(request.Take()).ToList(), sorted.Count, request.Skip(), request.Take());
        #endregion 
    }

    private async Task ResolveUserNamesAsync(List<PheDuyetListItemDto> rows, CancellationToken cancellationToken)
    {
        var portalIds = rows
            .SelectMany(r => new long?[] {
                r.NguoiTrinhId,
                r.NguoiDuyetId is > 0 ? r.NguoiDuyetId : null,
            })
            .Where(id => id is > 0)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (portalIds.Count == 0) {
            return;
        }

        var userMap = await _userMasterRepo.GetQueryableSet().AsNoTracking()
            .Where(u => u.UserPortalId != null && portalIds.Contains(u.UserPortalId.Value))
            .ToDictionaryAsync(u => u.UserPortalId!.Value, u => u.HoTen ?? string.Empty, cancellationToken);

        foreach (var row in rows) {
            row.NguoiTrinh = ResolveUserName(userMap, row.NguoiTrinhId);
            row.NguoiDuyet = ResolveUserName(userMap, row.NguoiDuyetId is > 0 ? row.NguoiDuyetId : null);
        }
    }

    private static string? ResolveUserName(IReadOnlyDictionary<long, string> userMap, long? portalId) =>
        portalId is > 0 && userMap.TryGetValue(portalId.Value, out var name) ? name : null;

    private async Task<List<PheDuyetListItemDto>> GetDuToanItems(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken)
    {
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
            .Select(e => new PheDuyetListItemDto
            {
                Id = e.Id,
                Type = PheDuyetEntityNames.PheDuyetDuToan,
                DuAnId = e.DuAnId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,
                TenBuoc = e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : string.Empty,
                TenGiaiDoan = e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : string.Empty,
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

    private async Task<List<PheDuyetListItemDto>> GetHoSoDeXuatCapDoCnttItems(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken)
    {
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
            .Select(e => new PheDuyetListItemDto
            {
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

    private async Task<List<PheDuyetListItemDto>> GetHoSoMoiThauDienTuItems(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken)
    {
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
            .Select(e => new PheDuyetListItemDto
            {
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

    private async Task<List<PheDuyetListItemDto>> GetBaoCaoKetQuaKhaoSatItems(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken)
    {
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
            .Select(e => new PheDuyetListItemDto
            {
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
