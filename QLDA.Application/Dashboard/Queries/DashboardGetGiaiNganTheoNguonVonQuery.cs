using Aspose.Cells;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;

namespace QLDA.Application.Dashboard.Queries;

/// <summary>
/// Query thống kê giải ngân theo nguồn vốn theo năm
/// </summary>
public record DashboardGetGiaiNganTheoNguonVonQuery(int Nam)
    : IRequest<List<DashboardGiaiNganTheoNguonVonDto>>;


internal class DashboardGetGiaiNganTheoNguonVonQueryHandler(
    IServiceProvider serviceProvider)
    : DashboardGiaiNganBaseHandler(serviceProvider),
      IRequestHandler<
          DashboardGetGiaiNganTheoNguonVonQuery,
          List<DashboardGiaiNganTheoNguonVonDto>> {
    public async Task<List<DashboardGiaiNganTheoNguonVonDto>> Handle(
        DashboardGetGiaiNganTheoNguonVonQuery request,
        CancellationToken cancellationToken) {
        try {
            var firstDayOfYear = request.Nam > 0 ? new DateTimeOffset(request.Nam, 1, 1, 0, 0, 0, TimeSpan.Zero)
    : (DateTimeOffset?)null;
            var firstDayOfNextYear = firstDayOfYear?.AddYears(1);

            var validDuAnIds = _authManager.FilterVisible(_duAn.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
                .Where(x => !x.IsDeleted).Select(x => x.Id);
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
                                        ? khv.SoVon : khv.SoVonDieuChinh.Value)) ?? 0m,

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
                item.TongKeHoachVon = Math.Round(item.TongKeHoachVon / 1000000m,3);
                item.GiaTriGiaiNgan = Math.Round(item.GiaTriGiaiNgan / 1000000m, 3);
            }
            return result;
            //        var result = await GetBaseQuery(request.Nam)
            //                 .GroupBy(x => new
            //                 {
            //                     x.NguonVonId,
            //                     x.TenNguonVon
            //                 })
            //.Select(g => new {
            //    g.Key.NguonVonId,
            //    g.Key.TenNguonVon,
            //    GiaTriVon = g.Sum(x => x.KeHoachVon),
            //    GiaTriGiaiNgan = g.Sum(x => x.GiaiNgan)
            //})
            //.ToListAsync(cancellationToken);

            //        return result
            //            .Select(x => new DashboardGiaiNganTheoNguonVonDto {
            //                NguonVonId = x.NguonVonId,
            //                TenNguonVon = x.TenNguonVon,
            //                TongKeHoachVon = x.GiaTriVon / 1_000_000m,
            //                GiaTriGiaiNgan = x.GiaTriGiaiNgan / 1_000_000m
            //            })
            //            .ToList();
        } catch (Exception exception) {
            Serilog.Log.Error("DashboardGetGiaiNganTheoNguonVonQuery năm {Nam} - error {Loi}", request.Nam, exception.Message);
            throw;
        }
    }
}



