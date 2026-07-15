namespace QLDA.Application.Common.Constants;

/// <summary>
/// Constants for E_ManHinh (DanhMucManHinh) - Screen types in project management workflow
/// </summary>
public static class ManHinhConst {
    // Array constants with QUYETDINHKEHOACH first, then default order
    public static readonly string[] MAN_HINHS_ORDERED = {
        QUYETDINHKEHOACH,//Kế hoạch lựa chọn nhà thầu

        // Default order screens
        GOITHAU,
        HOPDONG,
        THONGTINDUAN,
        PHEDUYETDUTOAN,
        VANBANCHUTRUONG,
        KHOKHANVUONGMAC,
        KETQUATRUNGTHAU,
        BAOCAOTIENDO,
        BIENBANGIAONHIEMVU,
        PHULUCHOPDONG,
        NGHIEMTHU,
        THANHTOAN,
        QUYETDINHGIAOVON,
        QUYETDINHDUYETQUYETTOAN,
        QUYETDINHDUYETKHLCNT,
        QUYETDINHTLBENMOITHAU,
        QUYETDINHLAPHOIDONGTHAMDINH,
        BAOCAOBANGIAOSANPHAM,
        BAOCAOBAOHANHSANPHAM,
        VANBANPHAPLY,
        DANGTAIKEHOACHLCNT
    };

    // Individual constants
    public const string GOITHAU = "GOITHAU";
    public const string HOPDONG = "HOPDONG";
    public const string THONGTINDUAN = "THONGTINDUAN";
    public const string PHEDUYETDUTOAN = "PHEDUYETDUTOAN";
    public const string VANBANCHUTRUONG = "VANBANCHUTRUONG";
    public const string KHOKHANVUONGMAC = "KHOKHANVUONGMAC";
    public const string KETQUATRUNGTHAU = "KETQUATRUNGTHAU";
    public const string BAOCAOTIENDO = "BAOCAOTIENDO";
    public const string BIENBANGIAONHIEMVU = "BIENBANGIAONHIEMVU";
    public const string PHULUCHOPDONG = "PHULUCHOPDONG";
    public const string NGHIEMTHU = "NGHIEMTHU";
    public const string THANHTOAN = "THANHTOAN";
    public const string QUYETDINHGIAOVON = "QUYETDINHGIAOVON";
    public const string QUYETDINHDUYETQUYETTOAN = "QUYETDINHDUYETQUYETTOAN";
    public const string QUYETDINHDUYETKHLCNT = "QUYETDINHDUYETKHLCNT";
    public const string QUYETDINHTLBENMOITHAU = "QUYETDINHTLBENMOITHAU";
    public const string QUYETDINHLAPHOIDONGTHAMDINH = "QUYETDINHLAPHOIDONGTHAMDINH";
    public const string BAOCAOBANGIAOSANPHAM = "BAOCAOBANGIAOSANPHAM";
    public const string BAOCAOBAOHANHSANPHAM = "BAOCAOBAOHANHSANPHAM";
    public const string QUYETDINHKEHOACH = "QUYETDINHKEHOACH";
    public const string VANBANPHAPLY = "VANBANPHAPLY";
    public const string DANGTAIKEHOACHLCNT = "DANGTAIKEHOACHLCNT";

    /// <summary>
    /// Get default Stt for a screen Ten from MAN_HINHS_ORDERED. Returns int.MaxValue if not found.
    /// </summary>
    public static int GetDefaultStt(string? ten) {
        if (ten == null) return int.MaxValue;
        var index = Array.IndexOf(MAN_HINHS_ORDERED, ten);
        return index >= 0 ? index + 1 : int.MaxValue;
    }
}

/// <summary>
/// Ordering extensions for ManHinh entities — uses Stt if any is set, else falls back to ManHinhConst.MAN_HINHS_ORDERED
/// </summary>
public static class ManHinhOrderingExtensions {
    // Pre-built lookup: Ten → default index
    private static int DefaultIndex(string? ten) => ManHinhConst.GetDefaultStt(ten);

    /// <summary>
    /// Order DanhMucManHinh by Stt if any Stt is non-null, else by ManHinhConst.MAN_HINHS_ORDERED
    /// </summary>
    public static IOrderedEnumerable<DanhMucManHinh> OrderByDefault(this IEnumerable<DanhMucManHinh> items) {
        var list = items as IList<DanhMucManHinh> ?? items.ToList();
        return list.Any(e => e.Stt.HasValue)
            ? list.OrderBy(e => e.Stt ?? int.MaxValue)
            : list.OrderBy(e => DefaultIndex(e.Ten));
    }

    /// <summary>
    /// Order DanhMucBuocManHinh by Stt if any Stt is non-null, else by ManHinhConst.MAN_HINHS_ORDERED (via ManHinh.Ten)
    /// </summary>
    public static IOrderedEnumerable<DanhMucBuocManHinh> OrderByDefault(this IEnumerable<DanhMucBuocManHinh> items) {
        var list = items as IList<DanhMucBuocManHinh> ?? items.ToList();
        return list.Any(e => e.Stt.HasValue)
            ? list.OrderBy(e => e.Stt ?? int.MaxValue)
            : list.OrderBy(e => DefaultIndex(e.ManHinh?.Ten));
    }

    /// <summary>
    /// Order DuAnBuocManHinh by Stt if any Stt is non-null, else by ManHinhConst.MAN_HINHS_ORDERED (via ManHinh.Ten)
    /// </summary>
    public static IOrderedEnumerable<DuAnBuocManHinh> OrderByDefault(this IEnumerable<DuAnBuocManHinh> items) {
        var list = items as IList<DuAnBuocManHinh> ?? items.ToList();
        return list.Any(e => e.Stt.HasValue)
            ? list.OrderBy(e => e.Stt ?? int.MaxValue)
            : list.OrderBy(e => DefaultIndex(e.ManHinh?.Ten));
    }
}
