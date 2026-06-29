namespace QLDA.Domain.Entities;

/// <summary>
/// Quyết định duyệt - Kế hoạch lựa chọn nhà thầu
/// </summary>
public class QuyetDinhDuyetKHLCNT :  Entity<Guid>, IAggregateRoot
{

    public Guid? KeHoachLuaChonNhaThauId { get; set; }
    /// <summary>
    /// Số quyết định
    /// </summary>

    // Thêm khóa ngoại rõ ràng sang bảng văn bản
    public Guid? QuyetDinhId { get; set; }
    #region Navigation Properties

    public KeHoachLuaChonNhaThau? KeHoachLuaChonNhaThau { get; set; }
    public VanBanQuyetDinh VanBanQuyetDinh { get; set; }

    #endregion
}