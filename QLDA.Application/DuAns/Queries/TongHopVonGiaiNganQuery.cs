using Microsoft.EntityFrameworkCore;

using QLDA.Application.DuAns.DTOs;
using QLDA.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QLDA.Application.DuAns.Queries;

public record TongHopVonGiaiNganQuery(int Nam, int LoaiDuAnId)
    : AggregateRootPagination, IRequest<List<BaoCaoDuAnDto>>
{
    public bool IsNoTracking { get; set; } = true;
}

internal class TongHopVonGiaiNganQueryHandler
    : IRequestHandler<TongHopVonGiaiNganQuery, List<BaoCaoDuAnDto>>
{

    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IRepository<NghiemThu, Guid> _nghiemThu;
    private readonly IRepository<ThanhToan, Guid> _thanhToan;
    private readonly IRepository<KeHoachVon, Guid> _keHoachVon;
    private readonly IDapperRepository _dapper;

    public TongHopVonGiaiNganQueryHandler(IServiceProvider serviceProvider)
    {
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _nghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        _keHoachVon = serviceProvider.GetRequiredService<IRepository<KeHoachVon, Guid>>();
        _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
    }

    public async Task<List<BaoCaoDuAnDto>> Handle(
        TongHopVonGiaiNganQuery request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _duAn.GetQueryableSet()
                  .AsNoTracking()
                  .Where(e => !e.IsDeleted);
            #region v2      
            var result = await query
                .Where(d =>
                    _keHoachVon.GetQueryableSet().Any(k =>
                        !k.IsDeleted &&
                        k.Nam == request.Nam &&
                        k.DuAnId == d.Id
                    )
                    ||
                    _thanhToan.GetQueryableSet().Any(t =>
                        !t.IsDeleted &&
                        t.NgayHoaDon.HasValue &&
                        t.NgayHoaDon.Value.Year == request.Nam &&
                        t.DuAnId == d.Id
                    )
                )
                .Select(d => new BaoCaoDuAnDto
                {
                    Id = d.Id,
                    TenDuAn = d.TenDuAn,
                    MaDuAn = d.MaDuAn,
                    LoaiDuAnTheoNamId = d.LoaiDuAnTheoNamId,

                    KeHoachVon = _keHoachVon.GetQueryableSet()
                        .Where(k => !k.IsDeleted && k.Nam == request.Nam && k.DuAnId == d.Id)
                        .Sum(k =>
                            (k.SoVonDieuChinh ?? 0) != 0
                                ? (k.SoVonDieuChinh ?? 0)
                                : k.SoVon 
                        ),

                    GiaTriGiaiNgan = _thanhToan.GetQueryableSet()
                        .Where(t =>
                            !t.IsDeleted &&
                            t.DuAnId == d.Id &&
                            t.NgayHoaDon.HasValue &&
                            t.NgayHoaDon.Value.Year == request.Nam)
                        .Sum(t => (long?)t.GiaTri) ?? 0
                })
                .ToListAsync(cancellationToken);
          return new List<BaoCaoDuAnDto>(result);
        }
        catch (Exception ex)
        {

            throw;
        }
        #endregion

        #region v2      
        //var query = _duAn.GetQueryableSet()
        //      .AsNoTracking()
        //      .Where(e => !e.IsDeleted);
        ////    .ApplyDuAnVisibility(_userProvider, _policyProvider);
        //if (request.LoaiDuAnId > 0)
        //    query = query.Where(e => e.LoaiDuAnId == request.LoaiDuAnId);


        //var totalCount = await query.CountAsync();
        //// ===== MAIN QUERY =====
        //var result = await query
        //    .OrderByDescending(e => e.NgayBatDau)

        //    .Select(e => new BaoCaoDuAnDto
        //    {
        //        Id = e.Id,
        //        TenDuAn = e.TenDuAn,
        //        MaDuAn = e.MaDuAn,
        //        LoaiDuAnId = e.LoaiDuAnId,

        //        KeHoachVon = _keHoachVon.GetQueryableSet()
        //           .Where(x => !x.IsDeleted && x.DuAnId == e.Id && x.Nam == request.Nam)
        //           .Sum(x => (long?)(
        //               (x.SoVonDieuChinh ?? 0) != 0
        //                   ? x.SoVonDieuChinh
        //                   : x.SoVon
        //           )),

        //        GiaTriGiaiNgan = _thanhToan.GetQueryableSet()
        //            .Where(x => !x.IsDeleted && x.DuAnId == e.Id &&  x.NgayHoaDon.Value.Year == request.Nam)
        //            .Sum(x => (long?)x.GiaTri) ?? 0,

        //    })
        //    .ToListAsync(cancellationToken);
        #endregion
    }


}