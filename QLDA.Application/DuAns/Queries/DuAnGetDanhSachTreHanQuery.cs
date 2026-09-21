using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.WebApi.Models.DuAns;

namespace QLDA.Application.DuAns.Queries;

public static class DuAnQueryExtensions {
    public static IQueryable<DuAnBuoc> GetDanhSachTreHanQueryable(
        this IRepository<DuAnBuoc, int> duAnBuocRepo,
    DuAnSearchOverdueDto searchDto) {
        var query = duAnBuocRepo .GetQueryableSet().AsNoTracking()
            // Bước chưa xóa + dự án chưa xóa -> đã filter sẵn
            .Where(e =>    e.DuAn != null )
            // Có ngày dự kiến + ngày thực tế
            // và thực tế > dự kiến => trễ hạn
            .Where(e => e.NgayDuKienKetThuc.HasValue &&
                                  (  (e.NgayThucTeKetThuc.HasValue && e.NgayThucTeKetThuc.Value.Date > e.NgayDuKienKetThuc.Value.Date )))
                                //  || (!e.NgayThucTeKetThuc.HasValue && DateTimeOffset.UtcNow.Date > e.NgayDuKienKetThuc.Value.Date )))

            // Đơn vị phụ trách chính
            .WhereIf(  searchDto.DonViPhuTrachChinhId > 0,    e => e.DuAn!.DonViPhuTrachChinhId ==       searchDto.DonViPhuTrachChinhId)
            // Dự kiến bắt đầu >= từ ngày
            .WhereIf(searchDto.DuKienTuNgay.HasValue,
                                e => e.NgayDuKienBatDau >= searchDto.DuKienTuNgay!.Value.ToStartOfDayUtc())
            // Dự kiến bắt đầu <= đến ngày
            .WhereIf(searchDto.DuKienDenNgay.HasValue,
                                e => e.NgayDuKienBatDau!.Value <=    searchDto.DuKienDenNgay!.Value.ToEndOfDayUtc())
            // Bước
            .WhereIf( searchDto.BuocId > 0,
                e => e.BuocId == searchDto.BuocId)
            // Search
            .WhereGlobalFilter(searchDto, e => e.DuAn!.TenDuAn, e => e.Buoc!.Ten);
        return query;
    }
}
