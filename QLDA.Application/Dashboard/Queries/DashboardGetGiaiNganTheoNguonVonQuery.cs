using Aspose.Cells;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;

namespace QLDA.Application.Dashboard.Queries;

/// <summary>
/// Query thống kê giải ngân theo nguồn vốn theo năm
/// </summary>
public record DashboardGetGiaiNganTheoNguonVonQuery(int Nam) : IRequest<List<DashboardGiaiNganTheoNguonVonDto>>;

internal class DashboardGetGiaiNganTheoNguonVonQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DashboardGetGiaiNganTheoNguonVonQuery, List<DashboardGiaiNganTheoNguonVonDto>> {

    private readonly IDapperRepository _dapper = serviceProvider.GetRequiredService<IDapperRepository>();

    private readonly IRepository<ThanhToan, Guid> _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    private readonly IRepository<KeHoachVon, Guid> _keHoachVon = serviceProvider.GetRequiredService<IRepository<KeHoachVon, Guid>>();
    private readonly IRepository<DuAn, Guid> _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<DanhMucNguonVon, int> _dmNguonVon = serviceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();
    private readonly IAuthorizationManager _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
    public async Task<List<DashboardGiaiNganTheoNguonVonDto>> Handle(
        DashboardGetGiaiNganTheoNguonVonQuery request, CancellationToken cancellationToken) {
        try {

       var firstDayOfYear = new DateTimeOffset(request.Nam, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var firstDayOfNextYear = firstDayOfYear.AddYears(1);
        var validDuAnIds = _authManager.FilterVisible(_duAn.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
            .Where(e => !e.IsDeleted)
            .Select(e => e.Id);

       
         var query = from nvId in _dmNguonVon.GetQueryableSet().Select(x => x.Id)
                     join nv in _dmNguonVon.GetQueryableSet().Where(x => !x.IsDeleted && x.Used)
                             on nvId equals nv.Id into nvGroup
                        from nv in nvGroup.DefaultIfEmpty()
                        select new DashboardGiaiNganTheoNguonVonDto {
                            NguonVonId = nvId,
                            TenNguonVon = nv != null ? nv.Ten : string.Empty,
                         //   Nam = request.Nam,
                            TongKeHoachVon = _keHoachVon.GetQueryableSet()
                                .Where(khv => !khv.IsDeleted
                                           && khv.NguonVonId == nvId
                                           && validDuAnIds.Contains(khv.DuAnId)
                                           && (request.Nam <= 0 || khv.Nam == request.Nam))
                                .Sum(khv => (decimal?)((!khv.SoVonDieuChinh.HasValue || khv.SoVonDieuChinh.Value == 0)
                                        ? khv.SoVon  : khv.SoVonDieuChinh.Value  )) ?? 0m,

                            GiaTriGiaiNgan = _thanhToan.GetQueryableSet()
                                .Where(tt => !tt.IsDeleted
                                          && tt.NghiemThu != null
                                          && tt.NghiemThu.HopDong != null
                                          && !tt.NghiemThu.HopDong.IsDeleted
                                          && tt.NghiemThu.HopDong.GoiThau != null
                                          && tt.NghiemThu.HopDong.GoiThau.NguonVonId == nvId
                                          && validDuAnIds.Contains(tt.NghiemThu.HopDong.GoiThau.DuAnId)
                                          && (request.Nam <= 0 || (tt.NgayHoaDon != null && tt.NgayHoaDon >= firstDayOfYear && tt.NgayHoaDon < firstDayOfNextYear)))
                                .Sum(tt => (decimal?)tt.GiaTri) ?? 0m
                        };
            var result = await query.ToListAsync(cancellationToken);
            foreach (var item in result) {
                // Chia 1,000,000 và lấy tối đa 6 số thập phân
                item.TongKeHoachVon = Math.Round(item.TongKeHoachVon / 1000000m, 6);
                item.GiaTriGiaiNgan = Math.Round(item.GiaTriGiaiNgan / 1000000m, 6);
            }
            return result;
        } catch (Exception exception) {
            Serilog.Log.Error("DashboardGetGiaiNganTheoNguonVonQuery năm {Nam} - error {Loi}", request.Nam, exception.Message  );  
            throw;
        }  /*old ------
         const string sql = """
               WITH GiaiNganTheoNguonVon AS (
                   SELECT
                       gt.NguonVonId,
                       SUM(tt.GiaTri) AS GiaTriGiaiNgan,
                       SUM(hd.GiaTri) AS GiaTriHopDong
                   FROM dbo.ThanhToan tt
                   JOIN dbo.NghiemThu nt ON nt.Id = tt.NghiemThuId
                   JOIN dbo.HopDong hd ON hd.Id = nt.HopDongId
                   JOIN dbo.GoiThau gt ON gt.Id = hd.GoiThauId
                   WHERE tt.IsDeleted = 0
                     AND hd.IsDeleted = 0
                     AND YEAR(tt.NgayHoaDon) = @Nam
                   GROUP BY gt.NguonVonId
               ),
               KeHoachVonTheoNguonVon AS (
                   SELECT
                       khv.NguonVonId,
                       SUM(
                           CASE
                               WHEN ISNULL(khv.SoVonDieuChinh, 0) <= 0 THEN khv.SoVon
                               ELSE khv.SoVonDieuChinh
                           END
                       ) AS TongKeHoachVon
                   FROM dbo.KeHoachVon khv
                   WHERE khv.Nam = @Nam
                     AND khv.IsDeleted = 0
                     AND khv.NguonVonId IS NOT NULL
                   GROUP BY khv.NguonVonId
               )
               SELECT
                   COALESCE(g.NguonVonId, k.NguonVonId) AS NguonVonId,
                   nv.Ten AS TenNguonVon,
                   ISNULL(g.GiaTriGiaiNgan, 0) AS GiaTriGiaiNgan,
                   ISNULL(g.GiaTriHopDong, 0) AS GiaTriHopDong,
                   ISNULL(k.TongKeHoachVon, 0) AS TongKeHoachVon
               FROM GiaiNganTheoNguonVon g
               FULL OUTER JOIN KeHoachVonTheoNguonVon k
                   ON g.NguonVonId = k.NguonVonId
               LEFT JOIN dbo.DmNguonVon nv
                   ON nv.Id = COALESCE(g.NguonVonId, k.NguonVonId)
               """;

           var result = await _dapper.QueryAsync<DashboardGiaiNganTheoNguonVonDto>(sql, new { request.Nam });
           return [.. result]; */
    }
}
