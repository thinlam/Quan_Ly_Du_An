namespace QLDA.Application.Dashboard.Queries;

/// <summary>
/// 9451  Biểu đồ   giải ngân vốn theo tháng group theo nguồn vốn, loại dự án, loại dự án theo năm  
/// </summary>

public record DashboardTienDoGiaiNganNguonVonQuery(TinhHinhGiaiNganSearchDto Req) : IRequest<List<TinhHinhGiaiNganDto>>;

internal class DashboardTienDoGiaiNganNguonVonQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DashboardTienDoGiaiNganNguonVonQuery, List<TinhHinhGiaiNganDto>>
{

    private readonly IDapperRepository _dapper = serviceProvider.GetRequiredService<IDapperRepository>();

    public async Task<List<TinhHinhGiaiNganDto>> Handle( DashboardTienDoGiaiNganNguonVonQuery request, CancellationToken cancellationToken)
    {
        var req = request.Req;

        var firstDayOfYear =
            new DateTimeOffset(req.Nam, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var firstDayOfNextYear = firstDayOfYear.AddYears(1);

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
    }

}

