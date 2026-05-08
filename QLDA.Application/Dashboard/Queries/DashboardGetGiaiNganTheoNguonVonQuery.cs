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
            SELECT gt.NguonVonId, nv.Ten AS TenNguonVon,
                SUM(tt.GiaTri) AS GiaTriGiaiNgan,
                SUM(hd.GiaTri) AS GiaTriHopDong
            FROM dbo.ThanhToan tt
            JOIN dbo.NghiemThu nt ON nt.Id = tt.NghiemThuId
            JOIN dbo.HopDong hd ON hd.Id = nt.HopDongId
            JOIN dbo.GoiThau gt ON gt.Id = hd.GoiThauId
            JOIN dbo.DmNguonVon nv ON nv.Id = gt.NguonVonId
            WHERE tt.IsDeleted = 0 AND hd.IsDeleted = 0
            AND YEAR(tt.NgayHoaDon) = @Nam
            GROUP BY gt.NguonVonId, nv.Ten
            """;

        var result = await _dapper.QueryAsync<DashboardGiaiNganTheoNguonVonDto>(sql, new { request.Nam });
        return [.. result];
    }
}
