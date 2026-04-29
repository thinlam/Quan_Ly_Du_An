namespace QLDA.Domain.Entities.DanhMuc;

/// <summary>
/// Danh mục quyền - catalog of all permission keys in the system
/// </summary>
public class DanhMucQuyen : DanhMuc<int>, IAggregateRoot, IMayHaveStt {
    public int? Stt { get; set; }

    /// <summary>
    /// Nhóm quyền (DuAn, GoiThau, HopDong, VanBan, ThanhToan, ...)
    /// </summary>
    public string? NhomQuyen { get; set; }

    #region Navigation Properties

    public ICollection<CauHinhVaiTroQuyen>? CauHinhVaiTroQuyens { get; set; } = [];

    #endregion
}
