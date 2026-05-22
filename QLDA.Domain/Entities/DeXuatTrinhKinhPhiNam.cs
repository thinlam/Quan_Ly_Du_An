using QLDA.Domain.Enums;
using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

public class DeXuatTrinhKinhPhiNam : IJunctionEntity<Guid, Guid>, IAggregateRoot {
    public Guid LeftId { get; set; }
  
    public Guid RightId { get; set; }

    #region Navigation Properties

    public DeXuatNhuCauKinhPhiNam? DeXuatNhuCauKinhPhi { get; set; }

    #endregion
}
