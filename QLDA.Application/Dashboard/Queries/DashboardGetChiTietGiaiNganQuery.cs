using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;

namespace QLDA.Application.Dashboard.Queries;

/// <summary>
/// Query chi tiết giải ngân theo năm và nguồn vốn
/// </summary>
public record DashboardGetChiTietGiaiNganQuery(int Nam, int? NguonVonId = null) : IRequest<List<DashboardChiTietGiaiNganDto>>;

internal class DashboardGetChiTietGiaiNganQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DashboardGetChiTietGiaiNganQuery, List<DashboardChiTietGiaiNganDto>> {

    private readonly IDapperRepository _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
    private readonly IRepository<DuAn, Guid>  _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<HopDong, Guid>  _hopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
    private readonly IRepository<ThanhToan, Guid> _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    private readonly IAuthorizationManager _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<List<DashboardChiTietGiaiNganDto>> Handle(
        DashboardGetChiTietGiaiNganQuery request, CancellationToken cancellationToken) {
        var firstDayOfYear = request.Nam > 0 ? new DateTimeOffset(request.Nam, 1, 1, 0, 0, 0, TimeSpan.Zero) : (DateTimeOffset?)null;
        var firstDayOfNextYear = firstDayOfYear?.AddYears(1);

        var queryable = _authManager.FilterVisible(_hopDong.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
            .Where(h => !h.IsDeleted  && h.DuAn != null  && !h.DuAn.IsDeleted)
            .WhereIf(request.NguonVonId.HasValue && request.NguonVonId > 0,
                h => h.GoiThau != null && h.GoiThau.NguonVonId == request.NguonVonId);
        var result = await queryable
               .Select(h => new DashboardChiTietGiaiNganDto {
                   TenDuAn = h.DuAn!.TenDuAn,
                   GiaTriHopDong = Math.Round((h.GiaTri ?? 0m) / 1000000m, 6),
                   GiaTriGiaiNgan = Math.Round((h.NghiemThus!
                        .Where(n => !n.IsDeleted && n.ThanhToan != null)
                        .Select(n => n.ThanhToan!)
                        .Where(t => !t.IsDeleted  && (request.Nam <= 0
                                                || (t.NgayHoaDon >= firstDayOfYear && t.NgayHoaDon < firstDayOfNextYear)))
                        .Sum(t => (decimal?)t.GiaTri) ?? 0m) / 1000000m, 6),
                   Ngay = h.NgayKy,
                   TrangThaiGiaiNgan = h.NghiemThus!
                        .Where(n => !n.IsDeleted && n.ThanhToan != null)
                        .Select(n => n.ThanhToan!)
                        .Any(t => !t.IsDeleted && (t.GiaTri ?? 0) > 0)    ? "Đã giải ngân"  : "Chưa giải ngân" })
               .ToListAsync(cancellationToken);
        return result;
        //var queryable = _authManager.FilterVisible(_thanhToan.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
        //    .Include(e => e.DuAn).Include(x => x!.NghiemThu).ThenInclude(x => x!.HopDong).ThenInclude(c => c!.GoiThau)
        //    .Include(e => e.NghiemThu).ThenInclude(x => x!.HopDong)
        //    .Where(e => !e.DuAn!.IsDeleted).Where(e => !e!.IsDeleted)
        //    .WhereIf(request.NguonVonId > 0, e => e.NghiemThu!.HopDong!.GoiThau!.NguonVonId == request.NguonVonId    ) ;
        // var result = await queryable
        //.Select(e => new DashboardChiTietGiaiNganDto {
        //    TenDuAn = e.DuAn!.TenDuAn,
        //    GiaTriGiaiNgan = e.GiaTri,
        //    GiaTriHopDong = e.NghiemThu!.HopDong!.GiaTri,
        //    Ngay = e.NgayHoaDon,
        //    TrangThaiGiaiNgan = e.GiaTri > 0 ? "Đã giải ngân" : "Chưa giải ngân"
        //})
        //.ToListAsync();


        /*
        var sql = """
            SELECT da.TenDuAn,
                tt.GiaTri AS GiaTriGiaiNgan,
                hd.GiaTri AS GiaTriHopDong,
                tt.NgayHoaDon AS Ngay,
                CASE WHEN tt.GiaTri > 0 THEN N'Đã giải ngân' ELSE N'Chưa giải ngân' END AS TrangThaiGiaiNgan
            FROM dbo.ThanhToan tt
            JOIN dbo.NghiemThu nt ON nt.Id = tt.NghiemThuId
            JOIN dbo.HopDong hd ON hd.Id = nt.HopDongId
            JOIN dbo.GoiThau gt ON gt.Id = hd.GoiThauId
            JOIN dbo.DuAn da ON da.Id = gt.DuAnId
            WHERE tt.IsDeleted = 0 AND hd.IsDeleted = 0
            AND YEAR(tt.NgayHoaDon) = @Nam
            """;

        if (request.NguonVonId.HasValue) {
            sql += " AND gt.NguonVonId = @NguonVonId";
        }

        var result = await _dapper.QueryAsync<DashboardChiTietGiaiNganDto>(sql, new { request.Nam, request.NguonVonId });
        return [.. result];
*/
    }
}
