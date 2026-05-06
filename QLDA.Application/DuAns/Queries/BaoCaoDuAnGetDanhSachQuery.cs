using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Extensions;
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

        var query = _duAn.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted);
        //    .ApplyDuAnVisibility(_userProvider, _policyProvider);

        // ===== FILTER (giữ như bạn, rút gọn lại) =====
        if (!string.IsNullOrEmpty(search.TenDuAn))
        {
            var keyword = search.TenDuAn.Trim();
            query = query.Where(e => EF.Functions.Like(e.TenDuAn!, $"%{keyword}%"));
        }

        if (search.ThoiGianKhoiCong > 0)
            query = query.Where(e => e.ThoiGianKhoiCong == search.ThoiGianKhoiCong);

        if (search.ThoiGianHoanThanh > 0)
            query = query.Where(e => e.ThoiGianHoanThanh == search.ThoiGianHoanThanh);

        if (search.LoaiDuAnTheoNamId > 0)
            query = query.Where(e => e.LoaiDuAnTheoNamId == search.LoaiDuAnTheoNamId);

        if (search.HinhThucDauTuId > 0)
            query = query.Where(e => e.HinhThucDauTuId == search.HinhThucDauTuId);
        var totalCount = await query.CountAsync();  
        // ===== MAIN QUERY =====
        var result = await query
            .OrderByDescending(e => e.NgayBatDau) 
            .Skip(search.Skip())
            .Take(search.Take())
            .Select(e => new BaoCaoDuAnDto
            {
                Id = e.Id,
                TenDuAn = e.TenDuAn,
                DonViPhuTrachChinhId = e.DonViPhuTrachChinhId,
                LoaiDuAnTheoNamId = e.LoaiDuAnTheoNamId,
                KhaiToanKinhPhi = e.KhaiToanKinhPhi,
                ThoiGianKhoiCong = e.ThoiGianKhoiCong,
                ThoiGianHoanThanh = e.ThoiGianHoanThanh,
                GiaiDoanHienTaiId =     e.GiaiDoanHienTaiId  ?? e.BuocHienTai.Buoc.GiaiDoanId,
                TienDo =  (e.GiaiDoanHienTai.Ten ?? "") +  ((e.GiaiDoanHienTai.Ten != null && e.BuocHienTai.TenBuoc != null) ? " - " : "") +        (e.BuocHienTai.TenBuoc ?? ""),
                GiaTriNghiemThu = _nghiemThu.GetQueryableSet()
                    .Where(x => !x.IsDeleted && x.DuAnId == e.Id)
                    .Sum(x => (long?)x.GiaTri),
                GiaTriGiaiNgan = _thanhToan.GetQueryableSet()
                    .Where(x => !x.IsDeleted && x.DuAnId == e.Id)
                    .Sum(x => (long?)x.GiaTri) ?? 0,
                SoQuyetDinhPheDuyet= e.SoQuyetDinhPheDuyet,
                NgayQuyetDinhPheDuyet = e.NgayQuyetDinhPheDuyet,
                DuToanBanDau = e.DuToans.OrderBy(x => x.Index).Select(x => x.SoDuToan) .FirstOrDefault(),
                DuToanDieuChinh = e.DuToans.OrderByDescending(x => x.Index) .Select(x => x.SoDuToan).FirstOrDefault(),
                HinhThucDauTuId = e.HinhThucDauTuId,
                LoaiDuAnId = e.LoaiDuAnId
            })
            .ToListAsync(cancellationToken);

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