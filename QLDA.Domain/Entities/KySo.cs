using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Enums;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng quản lý ký số
/// </summary>
public class KySo : Entity<Guid>, IAggregateRoot {
    /// <summary>
    /// Chủ sở hữu (long – FK sang bảng cần xác nhận)
    /// </summary>
    public long? ChuSoHuuId { get; set; }

    public string? Email { get; set; }

    /// <summary>
    /// FK → DanhMucChucVu
    /// </summary>
    public int? ChucVuId { get; set; }

    /// <summary>
    /// Phạm vi: CANHAN hoặc DONVI
    /// </summary>
    public EPhamViKySo? PhamVi { get; set; }

    public int? PhongBanId { get; set; }

    public string? SerialChungThu { get; set; }

    public string? ToChucCap { get; set; }

    public DateTime? HieuLucTu { get; set; }

    public DateTime? HieuLucDen { get; set; }

    /// <summary>
    /// FK → DanhMucPhuongThucKySo
    /// </summary>
    public int? PhuongThucKySoId { get; set; }

    #region Navigation Properties
    public DanhMucPhuongThucKySo? PhuongThucKySo { get; set; }
    public DanhMucChucVu? ChucVu { get; set; }
    #endregion
}