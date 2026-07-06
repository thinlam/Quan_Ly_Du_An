using QLDA.Domain.Entities;

namespace QLDA.Application.GoiThaus;

internal static class GoiThauTinhHinhDauThauQueryableExtensions
{
    /// <summary>
    /// Lọc dự án / giai đoạn — dùng chung list + print.
    /// <paramref name="giaiDoanId"/> null hoặc &lt;= 0 (gồm -1) → bỏ qua.
    /// </summary>
    public static IQueryable<GoiThau> ApplyTinhHinhDauThauFilters(
        this IQueryable<GoiThau> queryable,
        Guid? duAnId,
        int? giaiDoanId)
    {
        if (duAnId is Guid id && id != Guid.Empty)
            queryable = queryable.Where(e => e.DuAnId == id);

        if (giaiDoanId is > 0)
            queryable = queryable.Where(e => e.DuAn != null
                && e.DuAn.GiaiDoanHienTaiId == giaiDoanId.Value);

        return queryable;
    }

    /// <summary>
    /// Lọc theo query param <c>nam</c> trên API list — theo <c>DuAn.NgayBatDau</c>.
    /// </summary>
    public static IQueryable<GoiThau> ApplyTinhHinhDauThauNamFilter(
        this IQueryable<GoiThau> queryable,
        int? nam)
    {
        if (!nam.HasValue)
            return queryable;

        var firstDayOfYear = new DateTimeOffset(nam.Value, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var firstDayOfNextYear = firstDayOfYear.AddYears(1);
        return queryable.Where(e => e.DuAn != null
            && e.DuAn.NgayBatDau.HasValue
            && e.DuAn.NgayBatDau >= firstDayOfYear
            && e.DuAn.NgayBatDau < firstDayOfNextYear);
    }

    /// <summary>
    /// Lọc theo query param <c>namDuAn</c> trên API print — logic #9121
    /// (<c>ThoiGianKhoiCong</c> / <c>ThoiGianHoanThanh</c>).
    /// </summary>
    public static IQueryable<GoiThau> ApplyTinhHinhDauThauNamDuAnFilter(
        this IQueryable<GoiThau> queryable,
        int? namDuAn)
    {
        if (namDuAn is not > 0)
            return queryable;

        return queryable.Where(e => e.DuAn != null
            && namDuAn >= e.DuAn.ThoiGianKhoiCong
            && ((e.DuAn.ThoiGianHoanThanh == null && e.DuAn.ThoiGianKhoiCong == namDuAn)
                || namDuAn <= e.DuAn.ThoiGianHoanThanh));
    }

    /// <summary>
    /// Lọc tab trạng thái đấu thầu. Giá trị ngoài 1–3 → không lọc tab (list behavior).
    /// </summary>
    public static IQueryable<GoiThau> ApplyTinhHinhDauThauTabFilter(
        this IQueryable<GoiThau> queryable,
        int trangThai) =>
        trangThai switch
        {
            1 => queryable.Where(e => e.KetQuaTrungThau == null && e.HopDong == null),
            2 => queryable.Where(e => e.KetQuaTrungThau != null && e.HopDong == null),
            3 => queryable.Where(e => e.KetQuaTrungThau != null && e.HopDong != null),
            _ => queryable,
        };
}
