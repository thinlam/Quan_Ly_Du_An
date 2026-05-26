using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

/// <summary>
/// Junction: Tổng hợp KP năm ↔ Đề xuất nhu cầu kinh phí được trình
/// </summary>
public class DeXuatTrinhKinhPhiNam : IJunctionEntity<Guid, Guid>, IAggregateRoot {
    public Guid LeftId { get; set; }
    public Guid RightId { get; set; }

    #region Navigation Properties

    public DeXuatNhuCauKinhPhiNam? DeXuatNhuCauKinhPhiNam { get; set; }

    #endregion
}
