using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

using QLDA.Application.Common.Mapping;
using QLDA.Application.Providers;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Domain.Constants;

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
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationContext _authContext;
    private readonly IAppSettingsProvider _settings;

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
 
        var items = new List<PheDuyetListItemDto>();
        items.AddRange(await GetPheDuyetAll(request, cancellationToken));

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


        var sorted = items.OrderByDescending(i => i.NgayXuLyMoiNhat ?? DateTimeOffset.MinValue).ToList();
        return new PaginatedList<PheDuyetListItemDto>(sorted.Skip(request.Skip()).Take(request.Take()).ToList(), sorted.Count, request.Skip(), request.Take());
    }

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
    private async Task<List<PheDuyetListItemDto>> GetPheDuyetAll(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken)
    {
        var userId = _userProvider.Info.UserID;
        // Lưu ý: Tên biến hoặc logic phòng KHTC của bạn đang ghi là "!= _settings.PhongKHTCId" 
        // Nếu "là phòng KHTC" thì nên dùng dấu "==" nhé. Mình sửa lại thành == để đúng logic tên biến "isKHTC".
        bool isKHTC = _userProvider.Info.PhongBanID == _settings.PhongKHTCId;

        var pheDuyetQuery = _PheDuyetRepo.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted )
            .WhereIf(!string.IsNullOrEmpty(request.Type),e => e.EntityName == request.Type);
        var duAnQuery = _duAnRepo.GetQueryableSet().AsNoTracking();
        var pheDuyetHisQuery = _historyRepo.GetQueryableSet().AsNoTracking().Where(e => !e.IsDeleted && e.EntityName == request.Type && e.TrangThai.Ma== "ĐTr");
        var duAnBuocQuery = _duAnBuocRepo.GetQueryableSet().AsNoTracking();
       
        // 1. Viết câu lệnh Query cơ bản (Chưa có WHERE lọc quyền)
        var query = from e in pheDuyetQuery
                    join da in duAnQuery on e.DuAnId equals da.Id

                    join b in duAnBuocQuery on e.BuocId equals b.Id into buocGroup
                    from b in buocGroup.DefaultIfEmpty()

                    select new { e, da, b }; // Tạm thời select ra anonymous object để filter tiếp

        // 2. Tách biệt logic kiểm tra quyền bằng IF-ELSE của C#
        if (!isKHTC)
        {
            // Nếu KHÔNG PHẢI phòng KHTC thì mới ép Database chạy điều kiện lọc theo UserId
            query = query.Where(x => x.da.LanhDaoPhuTrachId == userId);
        }

        // 3. Cuối cùng mới Select ra DTO chuẩn
        var finalQuery = query.Select(x => new PheDuyetListItemDto
        {
            Id = x.e.Id,
            Type = request.Type,
            EntityId = x.e.EntityId.ToString(),
            EntityName = x.e.EntityName,
            DuAnId = x.e.DuAnId,
            TenDuAn = x.da != null ? x.da.TenDuAn : null,
            TenBuoc = x.b != null ? x.b.TenBuoc : null,
            TrichYeu = x.e.NoiDung,
            TrangThaiId = x.e.TrangThaiId,
            MaTrangThai = x.e.TrangThai != null && x.e.TrangThai.Ma != "LEG" ? x.e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
            TenTrangThai = x.e.TrangThai != null && x.e.TrangThai.Ma != "LEG" ? x.e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            NguoiDuyetId = x.e.TrangThai != null && x.e.TrangThai.Ma == "ĐD" ? x.e.NguoiXuLyId : 0,
            NguoiTrinhId = x.e.NguoiTrinhId,
            NgayXuLyMoiNhat = x.e.UpdatedAt
        }).OrderByDescending(x => x.Id); // Hoặc theo cột mong muốn thay cho p.Index nếu cần

        return await finalQuery.ToListAsync(cancellationToken);

    }
    
    private async Task<List<PheDuyetListItemDto>> GetPheDuyetAll2(PheDuyetGetDanhSachQuery request, CancellationToken cancellationToken)
    {
        // _duAnRepo join _PheDuyetRepo by DuAn.Id = PheDuyet.DuAnId -> get All PheDuyet has LanhDaoPhuTrachId = _userProvider.Info.UserId
        bool isKHTC = _userProvider.Info.PhongBanID != _settings.PhongKHTCId;
        var historyData = await _PheDuyetRepo.GetQueryableSet()
            .Where(h => h.EntityName == request.Type)

            .Select(h => new { h.EntityId, h.NgayXuLy })
            .ToListAsync(cancellationToken);

        var latestDates = historyData
            .GroupBy(h => h.EntityId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.NgayXuLy));
        var duAnBuoc = _duAnBuocRepo.GetQueryableSet().AsNoTracking();
        var pheDuyetQuery = _buocAuth.FilterVisibleChildEntities(_pheDuyetRepo.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId);
        //Màn hình này chỉ dành cho ban GD &phòng kế hoach
        //join DuAn get LanhDaoGiamDoc &PhongKHTC
        //Chỉ LanhDAoPhuTrachId = user.UserID || IsPhongKHTC

        var query =
                from e in pheDuyetQuery
                where !e.IsDeleted

                join b in duAnBuoc
                    on e.BuocId equals b.Id into buocGroup
                from b in buocGroup.DefaultIfEmpty()

                select new PheDuyetListItemDto
    {

        Id = e.Id,
        Type = request.Type,
        DuAnId = e.DuAnId,
        TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,
        TenBuoc = b != null ? b.TenBuoc : null,
        TrichYeu = e.NoiDung,
        TrangThaiId = e.TrangThaiId,
        MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
        TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
        NgayXuLyMoiNhat = e.UpdatedAt
    };

        var items = await query.ToListAsync(cancellationToken);
        //foreach (var item in items)
        //    item.NgayXuLyMoiNhat = latestDates.GetValueOrDefault(item.Id);

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
