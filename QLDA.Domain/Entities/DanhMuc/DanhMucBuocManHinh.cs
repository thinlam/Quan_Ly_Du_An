using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities.DanhMuc;

/// <summary>
/// Cân lưu lại id để load màn hình đã chọn
/// </summary>
public class DanhMucBuocManHinh : IJunctionEntity, IAggregateRoot {
    public int BuocId { get; set; }
    public int ManHinhId { get; set; }
    /// <summary>
    /// Thứ tự màn hình trong danh sách (để preserve thứ tự input từ user)
    /// </summary>
    public int Stt { get; set; }

    #region Navigation Properties

    public DanhMucBuoc? Buoc { get; set; }
    public DanhMucManHinh? ManHinh { get; set; }

    #endregion
}