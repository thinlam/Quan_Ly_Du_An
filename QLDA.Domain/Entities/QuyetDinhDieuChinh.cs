using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Quyết định điều chỉnh — hỗ trợ tất cả PheDuyet entities:
/// PheDuyetDuToan, PhanKhaiKinhPhi, HoSoDeXuatCapDoCntt, HoSoMoiThauDienTu
/// </summary>
public class QuyetDinhDieuChinh : Entity<Guid>, IAggregateRoot {
    /// <summary>
    /// Loại phe duyệt gốc (PheDuyetDuToan, PhanKhaiKinhPhi, etc.)
    /// </summary>
   // public string PheDuyetEntityName { get; set; } = default!;

    /// <summary>
    /// FK tới phe duyệt gốc
    /// </summary>
    //public Guid PheDuyetEntityId { get; set; }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }

    /// <summary>
    /// Số quyết định điều chỉnh
    /// </summary>
    public string? SoQuyetDinh { get; set; }

    public DateTimeOffset? NgayQuyetDinh { get; set; }

    public string? TrichYeu { get; set; }

    /// <summary>
    /// FK DanhMucLoaiDieuChinh
    /// </summary>
    public int LoaiDieuChinhId { get; set; }

    public string? LyDo { get; set; }

    /// <summary>
    /// FK Trạng thái phê duyệt điều chỉnh (7 trạng thái riêng)
    /// </summary>
    public int TrangThaiId { get; set; }

    /// <summary>
    /// Lần điều chỉnh — tự tính server-side: COUNT + 1 theo PheDuyetEntityId
    /// </summary>
    public int Lan { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DanhMucLoaiDieuChinh? LoaiDieuChinh { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    public ThongTinDieuChinhChiPhi? ThongTinDieuChinhChiPhi { get; set; }

    #endregion
}