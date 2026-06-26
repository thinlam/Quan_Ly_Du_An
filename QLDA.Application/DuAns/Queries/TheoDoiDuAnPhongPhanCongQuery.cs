using BuildingBlocks.CrossCutting.DateTimes;
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuAns.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAns.Queries;

public record TheoDoiDuAnPhongPhanCongQuery(TheoDoiDuAnPhongPhanCongSearchDto SearchDto)
    : IRequest<TheoDoiDuAnPhongPhanCongResultDto>;

internal class TheoDoiDuAnPhongPhanCongQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<TheoDoiDuAnPhongPhanCongQuery, TheoDoiDuAnPhongPhanCongResultDto>
{
    private readonly IRepository<DuAn, Guid> _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<DanhMucTrangThaiDuAn, int> _trangThaiDuAnRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiDuAn, int>>();
    private readonly IRepository<DmDonVi, long> _dmDonVi = serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<UserMaster, long> _userMaster = serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IAuthorizationManager _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
    private readonly IUserProvider _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    private readonly IDateTimeProvider _dateTimeProvider = serviceProvider.GetRequiredService<IDateTimeProvider>();

    public async Task<TheoDoiDuAnPhongPhanCongResultDto> Handle(
        TheoDoiDuAnPhongPhanCongQuery request,
        CancellationToken cancellationToken = default)
    {
        var search = request.SearchDto;
        var hoanThanh = await _trangThaiDuAnRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Ma == DanhMucTrangThaiDuAnCodes.HoanThanh, cancellationToken);

        ManagedException.ThrowIfNull(hoanThanh, "Không tìm thấy trạng thái dự án 'Đã hoàn thành'");

        var hoanThanhId = hoanThanh!.Id;
        var namHienTai = _dateTimeProvider.OffsetUtcNow.Year;
        var donViPhuTrachChinhId = ResolveDonViPhuTrachChinhId(search.DonViPhuTrachChinhId);

        var baseQuery = BuildBaseQuery(
            _authManager.FilterVisible(_duAn.GetQueryableSet(), AuthorizationResourceKeys.DuAn),
            search,
            donViPhuTrachChinhId);

        var stats = await baseQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                DaHoanThanh = g.Count(e => e.TrangThaiDuAnId == hoanThanhId),
                ConHan = g.Count(e => e.TrangThaiDuAnId != hoanThanhId
                    && (e.ThoiGianHoanThanh == null || namHienTai <= e.ThoiGianHoanThanh)),
                QuaHan = g.Count(e => e.TrangThaiDuAnId != hoanThanhId
                    && e.ThoiGianHoanThanh != null
                    && namHienTai > e.ThoiGianHoanThanh),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var daHoanThanh = stats?.DaHoanThanh ?? 0;
        var conHan = stats?.ConHan ?? 0;
        var quaHan = stats?.QuaHan ?? 0;
        var tongSo = conHan + quaHan + daHoanThanh;

        var listQuery = ApplyLoaiFilter(baseQuery, search.Loai, namHienTai, hoanThanhId);

        var paged = await listQuery
            .OrderBy(e => e.TenDuAn)
            .Select(e => new TheoDoiDuAnRow
            {
                Id = e.Id,
                MaDuAn = e.MaDuAn,
                TenDuAn = e.TenDuAn,
                ThoiGianKhoiCong = e.ThoiGianKhoiCong,
                ThoiGianHoanThanh = e.ThoiGianHoanThanh,
                TongMucDauTu = e.TongMucDauTu,
                LanhDaoPhuTrachId = e.LanhDaoPhuTrachId,
                DonViPhuTrachChinhId = e.DonViPhuTrachChinhId,
                HinhThucQuanLyDuAnId = e.HinhThucQuanLyDuAnId,
                HinhThucDauTuId = e.HinhThucDauTuId,
                TrangThaiDuAnId = e.TrangThaiDuAnId,
                HinhThucQuanLyDuAn = e.HinhThucQuanLy != null ? e.HinhThucQuanLy.Ten : null,
                HinhThucDauTu = e.HinhThucDauTu != null ? e.HinhThucDauTu.Ten : null,
            })
            .PaginatedListAsync(search.Skip(), search.Take(), cancellationToken);

        var items = await MapToDtoAsync(paged.Data, search, cancellationToken);

        return new TheoDoiDuAnPhongPhanCongResultDto
        {
            TongSoDuAn = tongSo,
            ConHan = conHan,
            QuaHan = quaHan,
            DaHoanThanh = daHoanThanh,
            DanhSach = new PaginatedList<TheoDoiDuAnPhongPhanCongDto>(
                items,
                paged.TotalRows,
                paged.PageNumber,
                search.PageSize),
        };
    }

    private long? ResolveDonViPhuTrachChinhId(long? donViPhuTrachChinhId)
    {
        if (donViPhuTrachChinhId is > 0)
            return donViPhuTrachChinhId;

        var phongBanId = _userProvider.Info.PhongBanID;
        return phongBanId > 0 ? phongBanId : donViPhuTrachChinhId;
    }

    private static IQueryable<DuAn> BuildBaseQuery(
        IQueryable<DuAn> query,
        TheoDoiDuAnPhongPhanCongSearchDto search,
        long? donViPhuTrachChinhId)
    {
        return query
            .AsNoTracking()
            .WhereIf(donViPhuTrachChinhId is > 0, e => e.DonViPhuTrachChinhId == donViPhuTrachChinhId)
            .WhereIf(search.TenDuAn.IsNotNullOrWhitespace(),
                e => e.TenDuAn!.ToLower().Contains(search.TenDuAn!.ToLower()))
            .WhereIf(search.MaDuAn.IsNotNullOrWhitespace(),
                e => e.MaDuAn!.ToLower().Contains(search.MaDuAn!.ToLower()))
            .WhereIf(search.NamDuAn > 0,
                e => search.NamDuAn >= e.ThoiGianKhoiCong
                     && ((e.ThoiGianHoanThanh == null && e.ThoiGianKhoiCong == search.NamDuAn)
                         || search.NamDuAn <= e.ThoiGianHoanThanh))
            .WhereGlobalFilter(search, e => e.TenDuAn, e => e.MaDuAn);
    }

    private static IQueryable<DuAn> ApplyLoaiFilter(
        IQueryable<DuAn> query,
        ETheoDoiDuAnPhongPhanCongLoai loai,
        int namHienTai,
        int hoanThanhId)
    {
        return loai switch
        {
            ETheoDoiDuAnPhongPhanCongLoai.DaHoanThanh => query.Where(e => e.TrangThaiDuAnId == hoanThanhId),
            ETheoDoiDuAnPhongPhanCongLoai.ConHan => query.Where(e => e.TrangThaiDuAnId != hoanThanhId
                && (e.ThoiGianHoanThanh == null || namHienTai <= e.ThoiGianHoanThanh)),
            ETheoDoiDuAnPhongPhanCongLoai.QuaHan => query.Where(e => e.TrangThaiDuAnId != hoanThanhId
                && e.ThoiGianHoanThanh != null
                && namHienTai > e.ThoiGianHoanThanh),
            _ => query,
        };
    }

    private async Task<List<TheoDoiDuAnPhongPhanCongDto>> MapToDtoAsync(
        List<TheoDoiDuAnRow> rows,
        TheoDoiDuAnPhongPhanCongSearchDto search,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return [];

        var lanhDaoIds = rows.Where(r => r.LanhDaoPhuTrachId.HasValue)
            .Select(r => r.LanhDaoPhuTrachId!.Value)
            .Distinct()
            .ToList();
        var donViIds = rows.Where(r => r.DonViPhuTrachChinhId.HasValue)
            .Select(r => r.DonViPhuTrachChinhId!.Value)
            .Distinct()
            .ToList();

        var lanhDaoDict = lanhDaoIds.Count == 0
            ? new Dictionary<long, string>()
            : await _userMaster.GetQueryableSet()
                .AsNoTracking()
                .Where(u => lanhDaoIds.Contains(u.Id))
                .Select(u => new { u.Id, u.HoTen })
                .ToDictionaryAsync(u => u.Id, u => u.HoTen ?? string.Empty, cancellationToken);

        var donViDict = donViIds.Count == 0
            ? new Dictionary<long, string>()
            : await _dmDonVi.GetQueryableSet()
                .AsNoTracking()
                .Where(dv => donViIds.Contains(dv.Id))
                .Select(dv => new { dv.Id, dv.TenDonVi })
                .ToDictionaryAsync(dv => dv.Id, dv => dv.TenDonVi ?? string.Empty, cancellationToken);

        var sttOffset = Math.Max(search.PageIndex - 1, 0) * search.PageSize;

        return rows.Select((row, index) => new TheoDoiDuAnPhongPhanCongDto
        {
            Stt = sttOffset + index + 1,
            Id = row.Id,
            MaDuAn = row.MaDuAn,
            TenDuAn = row.TenDuAn,
            ThoiGianThucHien = FormatThoiGianThucHien(row.ThoiGianKhoiCong, row.ThoiGianHoanThanh),
            HinhThucQuanLyDuAn = row.HinhThucQuanLyDuAn,
            HinhThucDauTu = row.HinhThucDauTu,
            TongMucDauTu = row.TongMucDauTu,
            LanhDaoPhuTrach = row.LanhDaoPhuTrachId.HasValue
                && lanhDaoDict.TryGetValue(row.LanhDaoPhuTrachId.Value, out var lanhDao)
                ? lanhDao
                : null,
            DonViPhuTrachChinh = row.DonViPhuTrachChinhId.HasValue
                && donViDict.TryGetValue(row.DonViPhuTrachChinhId.Value, out var donVi)
                ? donVi
                : null,
            HinhThucQuanLyDuAnId = row.HinhThucQuanLyDuAnId,
            HinhThucDauTuId = row.HinhThucDauTuId,
            DonViPhuTrachChinhId = row.DonViPhuTrachChinhId,
            ThoiGianKhoiCong = row.ThoiGianKhoiCong,
            ThoiGianHoanThanh = row.ThoiGianHoanThanh,
            TrangThaiDuAnId = row.TrangThaiDuAnId,
        }).ToList();
    }

    private static string? FormatThoiGianThucHien(int? tu, int? den)
    {
        if (tu is null && den is null)
            return null;
        if (tu is not null && den is not null)
            return $"{tu} - {den}";
        return (tu ?? den)?.ToString();
    }

    private sealed class TheoDoiDuAnRow
    {
        public Guid Id { get; init; }
        public string? MaDuAn { get; init; }
        public string? TenDuAn { get; init; }
        public int? ThoiGianKhoiCong { get; init; }
        public int? ThoiGianHoanThanh { get; init; }
        public long? TongMucDauTu { get; init; }
        public long? LanhDaoPhuTrachId { get; init; }
        public long? DonViPhuTrachChinhId { get; init; }
        public int? HinhThucQuanLyDuAnId { get; init; }
        public int? HinhThucDauTuId { get; init; }
        public int? TrangThaiDuAnId { get; init; }
        public string? HinhThucQuanLyDuAn { get; init; }
        public string? HinhThucDauTu { get; init; }
    }
}
