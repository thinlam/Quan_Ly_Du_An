using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuAns.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAns.Queries;

public record BaoCaoDuAnGetDanhSachQuery(BaoCaoDuAnSearchDto SearchDto)
    : AggregateRootPagination, IRequest<PaginatedList<BaoCaoDuAnDto>> {
    public bool IsNoTracking { get; set; } = true;
}

internal class BaoCaoDuAnGetDanhSachQueryHandler
    : IRequestHandler<BaoCaoDuAnGetDanhSachQuery, PaginatedList<BaoCaoDuAnDto>> {

    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IRepository<NghiemThu, Guid> _nghiemThu;
    private readonly IRepository<ThanhToan, Guid> _thanhToan;
    private readonly IDapperRepository _dapper;

    public BaoCaoDuAnGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _nghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
    }

    public async Task<PaginatedList<BaoCaoDuAnDto>> Handle(
        BaoCaoDuAnGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {

        var search = request.SearchDto;

        // Step 1: If period filter active, get matching DuAnIds via Dapper
        List<Guid>? periodFilteredIds = null;
        if (search.KyBaoCao != EKyBaoCao.None && search.NamFilter.HasValue) {
            periodFilteredIds = await GetDuAnIdsByPeriodAsync(search);
        }

        // Step 2: Build DuAn query with filters
        var queryable = _duAn.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.BuocHienTai)
            .Include(e => e.GiaiDoanHienTai)
            .WhereIf(search.TenDuAn.IsNotNullOrWhitespace(),
                e => e.TenDuAn!.ToLower().Contains(search.TenDuAn!.ToLower()))
            .WhereIf(search.ThoiGianKhoiCong > 0,
                e => e.ThoiGianKhoiCong == search.ThoiGianKhoiCong)
            .WhereIf(search.ThoiGianHoanThanh > 0,
                e => e.ThoiGianHoanThanh == search.ThoiGianHoanThanh)
            .WhereIf(search.LoaiDuAnTheoNamId > 0,
                e => e.LoaiDuAnTheoNamId == search.LoaiDuAnTheoNamId)
            .WhereIf(search.HinhThucDauTuId > 0,
                e => e.HinhThucDauTuId == search.HinhThucDauTuId)
            .WhereIf(search.LoaiDuAnId > 0,
                e => e.LoaiDuAnId == search.LoaiDuAnId)
            .WhereFunc(search.DonViPhuTrachChinhId.HasValue, q => q
                .WhereIf(search.DonViPhuTrachChinhId > 0, e => e.DonViPhuTrachChinhId == search.DonViPhuTrachChinhId)
                .WhereIf(search.DonViPhuTrachChinhId == -1, e => e.DonViPhuTrachChinhId == null))
            .WhereIf(periodFilteredIds != null, e => periodFilteredIds!.Contains(e.Id))
            .WhereGlobalFilter(search, e => e.TenDuAn);

        // Step 3: Paginate
        var totalCount = await queryable.CountAsync(cancellationToken);
        var duAnList = await queryable
            .Skip(search.Skip())
            .Take(search.Take())
            .ToListAsync(cancellationToken);

        var duAnIds = duAnList.Select(e => e.Id).ToList();

        // Step 4: Server-side DuToan first/last via Dapper
        var duToanDict = duAnIds.Count == 0
            ? new Dictionary<Guid, DuToanFirstLastRow>()
            : await GetDuToanFirstLastAsync(duAnIds);

        // Step 5: Server-side NghiemThu/ThanhToan aggregation
        var nghiemThuDict = duAnIds.Count == 0 ? new Dictionary<Guid, long>()
            : await _nghiemThu.GetQueryableSet()
                .AsNoTracking()
                .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
                .GroupBy(e => e.DuAnId)
                .Select(g => new { DuAnId = g.Key, Sum = g.Sum(x => x.GiaTri) })
                .ToDictionaryAsync(x => x.DuAnId, x => x.Sum, cancellationToken);

        var thanhToanDict = duAnIds.Count == 0 ? new Dictionary<Guid, long>()
            : await _thanhToan.GetQueryableSet()
                .AsNoTracking()
                .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
                .GroupBy(e => e.DuAnId)
                .Select(g => new { DuAnId = g.Key, Sum = g.Sum(x => (long)x.GiaTri.GetValueOrDefault()) })
                .ToDictionaryAsync(x => x.DuAnId, x => x.Sum, cancellationToken);

        // Step 6: Map to DTOs
        var result = duAnList.Select(duAn => {
            var giaTriNghiemThu = nghiemThuDict.GetValueOrDefault(duAn.Id, 0);
            var giaTriGiaiNgan = thanhToanDict.GetValueOrDefault(duAn.Id, 0);

            var tenGiaiDoan = duAn.GiaiDoanHienTai?.Ten ?? "";
            var tenBuoc = duAn.BuocHienTai?.TenBuoc ?? "";
            var tienDo = string.IsNullOrEmpty(tenGiaiDoan) && string.IsNullOrEmpty(tenBuoc)
                ? null
                : $"{tenGiaiDoan}{(string.IsNullOrEmpty(tenGiaiDoan) || string.IsNullOrEmpty(tenBuoc) ? "" : " - ")}{tenBuoc}";

            duToanDict.TryGetValue(duAn.Id, out var duToan);

            return new BaoCaoDuAnDto {
                Id = duAn.Id,
                TenDuAn = duAn.TenDuAn,
                DonViPhuTrachChinhId = duAn.DonViPhuTrachChinhId,
                LoaiDuAnTheoNamId = duAn.LoaiDuAnTheoNamId,
                KhaiToanKinhPhi = duAn.KhaiToanKinhPhi,
                ThoiGianKhoiCong = duAn.ThoiGianKhoiCong,
                ThoiGianHoanThanh = duAn.ThoiGianHoanThanh,
                DuToanBanDau = duToan?.DuToanBanDau,
                DuToanDieuChinh = duToan?.DuToanDieuChinh,
                NgayKyDuToanBanDau = duToan?.NgayKyDuToanBanDau,
                NgayKyDuToanDieuChinh = duToan?.NgayKyDuToanDieuChinh,
                SoQuyetDinhDuToan = duToan?.SoQuyetDinhDuToan,
                NamDuToan = duToan?.NamDuToanDieuChinh,
                TienDo = tienDo,
                GiaTriNghiemThu = giaTriNghiemThu > 0 ? giaTriNghiemThu : null,
                GiaTriGiaiNgan = giaTriGiaiNgan > 0 ? giaTriGiaiNgan : null,
                HinhThucDauTuId = duAn.HinhThucDauTuId,
                LoaiDuAnId = duAn.LoaiDuAnId,
                KyBaoCaoLabel = BuildKyBaoCaoLabel(search.KyBaoCao, duToan?.NgayKyDuToanDieuChinh)
            };
        }).ToList();

        return new PaginatedList<BaoCaoDuAnDto>(
            result, totalCount,
            search.PageIndex,
            search.PageSize);
    }

    /// <summary>
    /// Server-side query: first/last DuToan per DuAn using ROW_NUMBER window function
    /// Dự toán giao đầu năm: Lấy dòng đầu tiên
    /// Dự toán điều chỉnh/bổ sung: Lấy dòng cuối (chỉ nếu có > 2 bản dự toán)
    /// </summary>
    private async Task<Dictionary<Guid, DuToanFirstLastRow>> GetDuToanFirstLastAsync(List<Guid> duAnIds) {
        var sql = @"
            WITH RankedDuToan AS (
                SELECT dt.DuAnId, dt.SoDuToan, dt.NgayKyDuToan, dt.NamDuToan, dt.SoQuyetDinhDuToan,
                    ROW_NUMBER() OVER (PARTITION BY dt.DuAnId ORDER BY dt.NgayKyDuToan ASC) AS rn_asc,
                    ROW_NUMBER() OVER (PARTITION BY dt.DuAnId ORDER BY dt.NgayKyDuToan DESC) AS rn_desc,
                    COUNT(*) OVER (PARTITION BY dt.DuAnId) AS total_count
                FROM DuToan dt
                WHERE dt.IsDeleted = 0 AND dt.NgayKyDuToan IS NOT NULL
                  AND dt.DuAnId IN @duAnIds
            )
            SELECT
                first.DuAnId,
                first.SoDuToan AS DuToanBanDau,
                first.NgayKyDuToan AS NgayKyDuToanBanDau,
                first.NamDuToan AS NamDuToanBanDau,
                CASE WHEN last.total_count > 2 THEN last.SoDuToan ELSE NULL END AS DuToanDieuChinh,
                CASE WHEN last.total_count > 2 THEN last.NgayKyDuToan ELSE NULL END AS NgayKyDuToanDieuChinh,
                CASE WHEN last.total_count > 2 THEN last.SoQuyetDinhDuToan ELSE NULL END AS SoQuyetDinhDuToan,
                CASE WHEN last.total_count > 2 THEN last.NamDuToan ELSE NULL END AS NamDuToanDieuChinh
            FROM RankedDuToan first
            INNER JOIN RankedDuToan last
                ON first.DuAnId = last.DuAnId AND last.rn_desc = 1
            WHERE first.rn_asc = 1";

        var rows = await _dapper.QueryAsync<DuToanFirstLastRow>(sql, new { duAnIds });
        return rows.ToDictionary(r => r.DuAnId);
    }

    /// <summary>
    /// Server-side query: DuAnIds whose last DuToan falls in the specified period
    /// </summary>
    private async Task<List<Guid>> GetDuAnIdsByPeriodAsync(BaoCaoDuAnSearchDto search) {
        var dateCondition = search.KyBaoCao switch {
            EKyBaoCao.Thang => "MONTH(dt.NgayKyDuToan) = @Thang AND YEAR(dt.NgayKyDuToan) = @Nam",
            EKyBaoCao.Quy => "DATEPART(QUARTER, dt.NgayKyDuToan) = @Quy AND YEAR(dt.NgayKyDuToan) = @Nam",
            EKyBaoCao.Nam => "YEAR(dt.NgayKyDuToan) = @Nam",
            _ => "1=1"
        };

        var sql = $@"
            SELECT dt.DuAnId
            FROM DuToan dt
            INNER JOIN (
                SELECT DuAnId, MAX(NgayKyDuToan) AS MaxDate
                FROM DuToan
                WHERE IsDeleted = 0 AND NgayKyDuToan IS NOT NULL
                GROUP BY DuAnId
            ) last ON dt.DuAnId = last.DuAnId AND dt.NgayKyDuToan = last.MaxDate
            WHERE dt.IsDeleted = 0 AND {dateCondition}";

        var ids = await _dapper.QueryAsync<Guid>(sql, new {
            Nam = search.NamFilter,
            Quy = search.QuyFilter,
            Thang = search.ThangFilter
        });
        return ids.ToList();
    }

    private static string? BuildKyBaoCaoLabel(EKyBaoCao kyBaoCao, DateTimeOffset? lastDuToanDate) {
        if (kyBaoCao == EKyBaoCao.None || !lastDuToanDate.HasValue) return null;

        return kyBaoCao switch {
            EKyBaoCao.Thang => $"Tháng {lastDuToanDate.Value.Month}/{lastDuToanDate.Value.Year}",
            EKyBaoCao.Quy => $"Quý {(lastDuToanDate.Value.Month - 1) / 3 + 1}/{lastDuToanDate.Value.Year}",
            EKyBaoCao.Nam => $"Năm {lastDuToanDate.Value.Year}",
            _ => null
        };
    }
}