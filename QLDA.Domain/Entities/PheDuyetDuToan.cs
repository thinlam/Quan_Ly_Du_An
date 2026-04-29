using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Giai đoạn chuẩn bị đầu tư
/// </summary>
public class PheDuyetDuToan : VanBanQuyetDinh {
    public int? ChucVuId { get; set; }
    public long? GiaTriDuThau { get; set; }
    public int TrangThaiId { get; set; } = 1; // Default: Dự thảo

    /// <summary>
    /// USER_MASTER.UserPortalId
    /// </summary>
    public long? NguoiXuLyId { get; set; }

    /// <summary>
    /// USER_MASTER.UserPortalId
    /// </summary>
    public long? NguoiGiaoViecId { get; set; }

    #region Navigation Properties

    public DanhMucChucVu? ChucVu { get; set; }
    public DanhMucTrangThaiPheDuyetDuToan? TrangThai { get; set; }
    public ICollection<PheDuyetDuToanHistory>? Histories { get; set; } = [];

    #endregion
}