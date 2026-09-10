using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using System.Linq.Dynamic.Core;

namespace QLDA.Application.Dashboard.Queries;

/// <summary>
/// 9451  Biểu đồ   giải ngân vốn theo tháng group theo nguồn vốn, loại dự án, loại dự án theo năm  
/// </summary>

public record DashboardTienDoGiaiNganNguonVonQuery(TinhHinhGiaiNganSearchDto Req) : IRequest<List<TinhHinhGiaiNganDto>>;

internal class DashboardTienDoGiaiNganNguonVonQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DashboardTienDoGiaiNganNguonVonQuery, List<TinhHinhGiaiNganDto>>
{

    private readonly IDapperRepository _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
    private readonly IRepository<ThanhToan, Guid> _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    private readonly IAuthorizationManager    _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<List<TinhHinhGiaiNganDto>> Handle( DashboardTienDoGiaiNganNguonVonQuery request, CancellationToken cancellationToken)
    {
        var req = request.Req;
        var firstDayOfYear =   new DateTimeOffset(req.Nam, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var firstDayOfNextYear = firstDayOfYear.AddYears(1);

        var queryable = _authManager.FilterVisible(_thanhToan.GetQueryableSet(), AuthorizationResourceKeys.DuAn).Include(e => e.DuAn).ThenInclude(x => x!.DuAnNguonVons)
                    .Where(e => !e.DuAn!.IsDeleted)
                //    .Where(e => e.NghiemThu != null
                //&& e.NghiemThu.HopDong != null
                //&& !e.NghiemThu.HopDong.IsDeleted // hd.IsDeleted = 0
                //&& e.NghiemThu.HopDong.GoiThau != null
                //&& e.NghiemThu.HopDong.GoiThau.DuAn != null
                //&& !e.NghiemThu.HopDong.GoiThau.DuAn.IsDeleted)
                    .WhereIf(req.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == req.LoaiDuAnTheoNamId)
                    .WhereIf(req.LoaiDuAnId > 0, e => e.DuAn!.LoaiDuAnId == req.LoaiDuAnId)
                    .WhereIf(req.NguonVonId > 0, e => e.DuAn!.DuAnNguonVons!.Select(i => i.RightId).Contains(req.NguonVonId ?? 0))
                    .WhereIf(req.Nam > 0, e => e.NgayHoaDon >= firstDayOfYear && e.NgayHoaDon < firstDayOfNextYear)
                    ;
        var result = await queryable
    .GroupBy(e => new {
        NguonVonId = e.NghiemThu!.HopDong!.GoiThau!.NguonVonId,
        LoaiDuAnId = e.DuAn!.LoaiDuAnId,
        LoaiDuAnTheoNamId = e.DuAn!.LoaiDuAnTheoNamId,
        Nam = e.NgayHoaDon!.Value.Year,  
        Thang = e.NgayHoaDon!.Value.Month 
    })
    .Select(g => new TinhHinhGiaiNganDto {
        NguonVonId = g.Key.NguonVonId,
        LoaiDuAnId = g.Key.LoaiDuAnId,
        LoaiDuAnTheoNamId = g.Key.LoaiDuAnTheoNamId,
        Nam = g.Key.Nam,
        Thang = g.Key.Thang,
        GiaTriGiaiNgan = Math.Round( (g.Sum(x =>(decimal?)x.GiaTri)?? 0m ) / 1000000m, 6) 
    }) .ToListAsync();
     return result;
        /* old
         * 
        const string sql = """
        SELECT
            (SUM(tt.GiaTri)/1000000 ) AS GiaTriGiaiNgan,
            gt.NguonVonId,
            d.LoaiDuAnId,
            d.LoaiDuAnTheoNamId,
            YEAR(tt.NgayHoaDon) as Nam,
            MONTH(tt.NgayHoaDon) AS Thang
        FROM dbo.ThanhToan tt
        JOIN dbo.NghiemThu nt ON nt.Id = tt.NghiemThuId
        JOIN dbo.HopDong hd ON hd.Id = nt.HopDongId
        JOIN dbo.GoiThau gt ON gt.Id = hd.GoiThauId
        JOIN dbo.DuAn d ON d.Id = gt.DuAnId
        WHERE tt.IsDeleted = 0
            AND hd.IsDeleted = 0
            AND (@LoaiDuAnId IS NULL OR d.LoaiDuAnId = @LoaiDuAnId)
            AND (@LoaiDuAnTheoNamId IS NULL OR d.LoaiDuAnTheoNamId = @LoaiDuAnTheoNamId)
            AND (@NguonVonId IS NULL OR gt.NguonVonId = @NguonVonId)
            AND tt.NgayHoaDon >= @FirstDayOfYear
            AND tt.NgayHoaDon < @FirstDayOfNextYear
        GROUP BY
            gt.NguonVonId,
            d.LoaiDuAnTheoNamId,
            d.LoaiDuAnId,
            MONTH(tt.NgayHoaDon),YEAR(tt.NgayHoaDon) 
        """;

        var result = await _dapper.QueryAsync<TinhHinhGiaiNganDto>(
            sql,
            new
            {
                req.LoaiDuAnId,
                req.LoaiDuAnTheoNamId,
                req.NguonVonId,
                FirstDayOfYear = firstDayOfYear,
                FirstDayOfNextYear = firstDayOfNextYear
            });

        return [.. result];
         */

    }

}

