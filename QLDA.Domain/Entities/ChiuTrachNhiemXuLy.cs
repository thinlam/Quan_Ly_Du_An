using QLDA.Domain.Enums;

namespace QLDA.Domain.Entities;

public class DuAnChiuTrachNhiemXuLy : IJunctionEntity<Guid, long>, IAggregateRoot {
    public Guid LeftId { get; set; }
    /// <summary>
    /// DonViPhoiHop: Đơn vị phối hợp <br/>
    /// DonViTheoDoi: Đơn vị theo dõi <br/>
    /// </summary>
    public EChiuTrachNhiemXuLy Loai { get; set; } = EChiuTrachNhiemXuLy.DonViPhoiHop;
    public long RightId { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }

    #endregion
}
