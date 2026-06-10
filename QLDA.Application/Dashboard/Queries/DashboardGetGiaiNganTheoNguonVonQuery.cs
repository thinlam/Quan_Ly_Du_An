using QLDA.Domain.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.Dashboard.Queries;

/// <summary>
/// Query thống kê giải ngân theo nguồn vốn theo năm
/// </summary>
public record DashboardGetGiaiNganTheoNguonVonQuery(int Nam) : IRequest<List<DashboardGiaiNganTheoNguonVonDto>>;

internal class DashboardGetGiaiNganTheoNguonVonQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DashboardGetGiaiNganTheoNguonVonQuery, List<DashboardGiaiNganTheoNguonVonDto>> {

    private readonly IDapperRepository _dapper = serviceProvider.GetRequiredService<IDapperRepository>();

    public async Task<List<DashboardGiaiNganTheoNguonVonDto>> Handle(
        DashboardGetGiaiNganTheoNguonVonQuery request, CancellationToken cancellationToken) {

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
        return [.. result];
    }
}
