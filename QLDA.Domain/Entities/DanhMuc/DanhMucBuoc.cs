
namespace QLDA.Domain.Entities.DanhMuc;

/// <summary>
/// Danh mục bước
/// </summary>
/// <remarks>
/// Quy trình - bước - tình trạng dự án
/// </remarks>
public class DanhMucBuoc : MaterializedPathDanhMuc<int>, IAggregateRoot, IMayHaveStt
{
    public int QuyTrinhId { get; set; }
    public int? GiaiDoanId { get; set; }
    public new int? ParentId { get; set; }
    public int? Stt { get; set; }

    /// <summary>
    /// Là một tập hợp chuỗi các màn hình
    /// </summary>
    public string? PartialView { get; set; } = string.Empty;

    /*
     * Số ngày thực hiện dự kiến mỗi bước
     * Là một trường quan trọng kết hợp với ngày bắt đầu NgayBatDau ở bảng DuAn để tính và thứ tự treelist của bảng này
     * => tính ngày bắt đầu - kết thúc dự kiến trong dự án bước (DuAnBuoc)
     * Chú giải:
     * + bước đầu tiên ngày bắt đầu dự kiến là ngày bắt đầu dự kiến
     * + các bước tiếp theo, ngày bắt đầu dự kiến là ngày kết thúc dự kiến của bước trước đó cộng thêm 1 ngày
     * + ngày kết thúc bằng ngày dự kiến cộng ngày theo số ngày dự kiến
     * Lưu ý: Khi tính phải sắp xếp vị trí chúng dưới dạng treelist - sắp xếp theo level, số thứ  tự
     * ==================================================================
     * Ví dụ dự án có ngày bắt đầu: 20/11
     * duAnBuoc thứ 1 (sau khi sắp xếp treelist):
     * - duAnBuoc.NgayDuKienBatDau = duAn.ngayBatDau
     * - duAnBuoc.NgayDuKienKetThuc = duAn.ngayBatDau.AddDays(dmBuoc.SoNgayThucHien)
     * duAnBuoc thứ n
     * - duAnBuoc.NgayDuKienBatDau = duAnBuoc[n-1].NgayDuKienKetThuc.AddDays(1)
     * - duAnBuoc.NgayDuKienKetThuc = duAn.ngayBatDau.AddDays(dmBuoc.SoNgayThucHien)
     */
    /// <summary>
    /// go to declaration để xem comment nhé
    /// </summary>
    public int SoNgayThucHien { get; set; }


    #region Navigation Properties

    public DanhMucQuyTrinh? QuyTrinh { get; set; }
    public DanhMucGiaiDoan? GiaiDoan { get; set; }
    public ICollection<DanhMucBuocTrangThaiTienDo>? BuocTrangThaiTienDos { get; set; } = [];
    public ICollection<DuAnBuoc>? DuAnBuocs { get; set; } = [];
    public ICollection<DanhMucBuocManHinh>? BuocManHinhs { get; set; } = [];
    public ICollection<DanhMucBuocPhongBanPhoiHop>? BuocPhongBanPhoiHops { get; set; } = [];

    #endregion
}
