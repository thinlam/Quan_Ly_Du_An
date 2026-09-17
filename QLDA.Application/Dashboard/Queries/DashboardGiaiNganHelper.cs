using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.Dashboard.Queries;

internal static class DashboardGiaiNganHelper {
    /// <summary>
    /// Base Query lấy chi tiết giải ngân theo năm và nguồn vốn (kết hợp cả ThanhToan và KeHoachVon)
    /// Đảm bảo nguồn vốn có kế hoạch vốn nhưng chưa giải ngân vẫn hiển thị danh sách dự án
    /// </summary>
    public static async Task<List<DashboardChiTietGiaiNganDto>> GetChiTietGiaiNganAsync(
        IRepository<DuAn, Guid> duAnRepo,
        IRepository<KeHoachVon, Guid> keHoachVonRepo,
        IRepository<ThanhToan, Guid> thanhToanRepo,
        IRepository<HopDong, Guid> hopDongRepo,
        IAuthorizationManager authManager,
        int nam,
        int? nguonVonId,
        CancellationToken cancellationToken) {

        var firstDayOfYear = nam > 0 ? new DateTimeOffset(nam, 1, 1, 0, 0, 0, TimeSpan.Zero) : (DateTimeOffset?)null;
        var firstDayOfNextYear = firstDayOfYear?.AddYears(1);

        var validDuAnIds = await authManager.FilterVisible(duAnRepo.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
            .Where(x => !x.IsDeleted)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (validDuAnIds.Count == 0) {
            return [];
        }

        var result = new List<DashboardChiTietGiaiNganDto>();

        // 1. Lấy các khoản đã giải ngân (ThanhToan) trong năm theo NguonVonId
        var ttQuery = thanhToanRepo.GetQueryableSet()
            .Where(tt => !tt.IsDeleted
                      && tt.NghiemThu != null
                      && tt.NghiemThu.HopDong != null
                      && !tt.NghiemThu.HopDong.IsDeleted
                      && tt.NghiemThu.HopDong.GoiThau != null
                      && validDuAnIds.Contains(tt.NghiemThu.HopDong.GoiThau.DuAnId)
                      && (nam <= 0 || (tt.NgayHoaDon != null && tt.NgayHoaDon >= firstDayOfYear && tt.NgayHoaDon < firstDayOfNextYear)));

        if (nguonVonId.HasValue && nguonVonId > 0) {
            ttQuery = ttQuery.Where(tt => tt.NguonVonId.HasValue
                ? tt.NguonVonId == nguonVonId
                : tt.NghiemThu!.HopDong!.GoiThau!.NguonVonId == nguonVonId);
        }

        var daGiaiNganList = await ttQuery
            .Select(tt => new {
                DuAnId = tt.NghiemThu!.HopDong!.GoiThau!.DuAnId,
                TenDuAn = tt.NghiemThu.HopDong.GoiThau.DuAn != null ? tt.NghiemThu.HopDong.GoiThau.DuAn.TenDuAn : string.Empty,
                GiaTriHopDong = (decimal?)tt.NghiemThu.HopDong.GiaTri ?? 0m,
                GiaTriGiaiNgan = (decimal?)tt.GiaTri ?? 0m,
                Ngay = tt.NgayHoaDon ?? tt.NghiemThu.HopDong.NgayKy,
                NguonVonId = tt.NguonVonId ?? tt.NghiemThu.HopDong.GoiThau.NguonVonId
            })
            .ToListAsync(cancellationToken);

        foreach (var item in daGiaiNganList) {
            result.Add(new DashboardChiTietGiaiNganDto {
                TenDuAn = item.TenDuAn,
                GiaTriHopDong = Math.Round(item.GiaTriHopDong / 1000000m, 3),
                GiaTriGiaiNgan = Math.Round(item.GiaTriGiaiNgan / 1000000m, 3),
                Ngay = item.Ngay,
                TrangThaiGiaiNgan = item.GiaTriGiaiNgan > 0
            });
        }

        // 2. Lấy các dự án có Kế hoạch vốn trong năm nhưng chưa có giải ngân nào trong năm của nguồn vốn đó
        var duAnIdsDaGiaiNgan = daGiaiNganList.Select(x => x.DuAnId).Distinct().ToHashSet();

        var khvQuery = keHoachVonRepo.GetQueryableSet()
            .Where(khv => !khv.IsDeleted
                       && validDuAnIds.Contains(khv.DuAnId)
                       && (nam <= 0 || khv.Nam == nam)
                       && khv.NguonVonId != null);

        if (nguonVonId.HasValue && nguonVonId > 0) {
            khvQuery = khvQuery.Where(khv => khv.NguonVonId == nguonVonId);
        }

        var chuaGiaiNganRaw = await khvQuery
            .Where(khv => !duAnIdsDaGiaiNgan.Contains(khv.DuAnId))
            .Select(khv => new {
                khv.DuAnId,
                TenDuAn = khv.DuAn != null ? khv.DuAn.TenDuAn : string.Empty,
                khv.NgayKy
            })
            .ToListAsync(cancellationToken);

        var chuaGiaiNganDuAnList = chuaGiaiNganRaw
            .GroupBy(x => new { x.DuAnId, x.TenDuAn })
            .Select(g => new {
                g.Key.DuAnId,
                g.Key.TenDuAn,
                NgayKy = g.Max(x => x.NgayKy)
            })
            .ToList();

        // Lấy giá trị hợp đồng (nếu có) của các dự án chưa giải ngân này
        var duAnChuaGiaiNganIds = chuaGiaiNganDuAnList.Select(x => x.DuAnId).ToList();
        var hopDongValues = new Dictionary<Guid, decimal>();
        if (duAnChuaGiaiNganIds.Count > 0) {
            var hdQuery = hopDongRepo.GetQueryableSet()
                .Where(h => !h.IsDeleted && duAnChuaGiaiNganIds.Contains(h.DuAnId));
            if (nguonVonId.HasValue && nguonVonId > 0) {
                hdQuery = hdQuery.Where(h => h.GoiThau != null && h.GoiThau.NguonVonId == nguonVonId);
            }
            var hdRaw = await hdQuery
                .Select(h => new { h.DuAnId, GiaTri = (decimal?)h.GiaTri ?? 0m })
                .ToListAsync(cancellationToken);
            hopDongValues = hdRaw
                .GroupBy(x => x.DuAnId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.GiaTri));
        }

        foreach (var item in chuaGiaiNganDuAnList) {
            hopDongValues.TryGetValue(item.DuAnId, out var giaTriHd);
            result.Add(new DashboardChiTietGiaiNganDto {
                TenDuAn = item.TenDuAn,
                GiaTriHopDong = Math.Round(giaTriHd / 1000000m, 3),
                GiaTriGiaiNgan = 0m,
                Ngay = item.NgayKy,
                TrangThaiGiaiNgan = false
            });
        }

        return result;
    }

    /// <summary>
    /// Base Query lấy tổng hợp giải ngân theo nguồn vốn theo năm
    /// Đảm bảo số liệu tổng hợp khớp 100% với số liệu chi tiết
    /// </summary>
    public static async Task<List<DashboardGiaiNganTheoNguonVonDto>> GetGiaiNganTheoNguonVonAsync(
        IRepository<DuAn, Guid> duAnRepo,
        IRepository<DanhMucNguonVon, int> dmNguonVonRepo,
        IRepository<KeHoachVon, Guid> keHoachVonRepo,
        IRepository<ThanhToan, Guid> thanhToanRepo,
        IRepository<HopDong, Guid> hopDongRepo,
        IAuthorizationManager authManager,
        int nam,
        CancellationToken cancellationToken) {

        var firstDayOfYear = nam > 0 ? new DateTimeOffset(nam, 1, 1, 0, 0, 0, TimeSpan.Zero) : (DateTimeOffset?)null;
        var firstDayOfNextYear = firstDayOfYear?.AddYears(1);

        var validDuAnIds = await authManager.FilterVisible(duAnRepo.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
            .Where(x => !x.IsDeleted)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (validDuAnIds.Count == 0) {
            return [];
        }

        var nguonVons = await dmNguonVonRepo.GetQueryableSet()
            .Where(x => !x.IsDeleted && x.Used)
            .Select(x => new { x.Id, x.Ten })
            .ToListAsync(cancellationToken);

        // 1. Tổng kế hoạch vốn theo nguồn vốn
        var khvRaw = await keHoachVonRepo.GetQueryableSet()
            .Where(khv => !khv.IsDeleted
                       && khv.NguonVonId != null
                       && validDuAnIds.Contains(khv.DuAnId)
                       && (nam <= 0 || khv.Nam == nam))
            .Select(khv => new {
                NguonVonId = khv.NguonVonId!.Value,
                SoVon = (!khv.SoVonDieuChinh.HasValue || khv.SoVonDieuChinh.Value == 0)
                    ? khv.SoVon : khv.SoVonDieuChinh.Value
            })
            .ToListAsync(cancellationToken);

        var khvTotals = khvRaw
            .GroupBy(x => x.NguonVonId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.SoVon));

        // 2. Tổng giải ngân theo nguồn vốn (sử dụng tt.NguonVonId hoặc fallback sang gt.NguonVonId)
        var ttRaw = await thanhToanRepo.GetQueryableSet()
            .Where(tt => !tt.IsDeleted
                      && tt.NghiemThu != null
                      && tt.NghiemThu.HopDong != null
                      && !tt.NghiemThu.HopDong.IsDeleted
                      && tt.NghiemThu.HopDong.GoiThau != null
                      && validDuAnIds.Contains(tt.NghiemThu.HopDong.GoiThau.DuAnId)
                      && (nam <= 0 || (tt.NgayHoaDon != null && tt.NgayHoaDon >= firstDayOfYear && tt.NgayHoaDon < firstDayOfNextYear)))
            .Select(tt => new {
                NguonVonId = tt.NguonVonId ?? tt.NghiemThu!.HopDong!.GoiThau!.NguonVonId,
                GiaTri = (decimal?)tt.GiaTri ?? 0m
            })
            .ToListAsync(cancellationToken);

        var ttTotals = ttRaw
            .Where(x => x.NguonVonId != null)
            .GroupBy(x => x.NguonVonId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.GiaTri));

        // 3. Tổng giá trị hợp đồng theo nguồn vốn
        var hdRaw = await hopDongRepo.GetQueryableSet()
            .Where(hd => !hd.IsDeleted
                      && hd.GoiThau != null
                      && hd.GoiThau.NguonVonId != null
                      && validDuAnIds.Contains(hd.DuAnId))
            .Select(hd => new {
                NguonVonId = hd.GoiThau!.NguonVonId!.Value,
                GiaTri = (decimal?)hd.GiaTri ?? 0m
            })
            .ToListAsync(cancellationToken);

        var hdTotals = hdRaw
            .GroupBy(x => x.NguonVonId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.GiaTri));

        // 4. Tổng hợp danh sách theo nguồn vốn
        var result = new List<DashboardGiaiNganTheoNguonVonDto>();
        foreach (var nv in nguonVons) {
            khvTotals.TryGetValue(nv.Id, out var tongKhv);
            ttTotals.TryGetValue(nv.Id, out var giaTriGiaiNgan);
            hdTotals.TryGetValue(nv.Id, out var giaTriHopDong);

            // Hiển thị nếu có Kế hoạch vốn, hoặc Giải ngân, hoặc Hợp đồng
            if (tongKhv > 0 || giaTriGiaiNgan > 0 || giaTriHopDong > 0) {
                result.Add(new DashboardGiaiNganTheoNguonVonDto {
                    NguonVonId = nv.Id,
                    TenNguonVon = nv.Ten,
                    TongKeHoachVon = Math.Round(tongKhv / 1000000m, 3),
                    GiaTriGiaiNgan = Math.Round(giaTriGiaiNgan / 1000000m, 3),
                    GiaTriHopDong = Math.Round(giaTriHopDong / 1000000m, 3)
                });
            }
        }

        return result;
    }
}
