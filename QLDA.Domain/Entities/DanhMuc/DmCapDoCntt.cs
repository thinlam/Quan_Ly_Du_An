namespace QLDA.Domain.Entities.DanhMuc;

/// <summary>
/// Danh mục cấp độ CNTT
/// </summary>
public class DmCapDoCntt : DanhMuc<int>, IAggregateRoot, IMayHaveStt {
    public int? Stt { get; set; }

    /// <summary>
    /// Mã màu hiển thị UI, vd: #ca0464
    /// </summary>
    public string? MaMau { get; set; }

    #region Navigation Properties
    public List<HoSoDeXuatCapDoCntt>? HoSoDeXuatCapDoCntts { get; set; } = [];
    #endregion
}