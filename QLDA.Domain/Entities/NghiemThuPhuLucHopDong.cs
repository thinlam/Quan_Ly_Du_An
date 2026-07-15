namespace QLDA.Domain.Entities;

public class NghiemThuPhuLucHopDong : IJunctionEntity<Guid, Guid>, IAggregateRoot
{
    public Guid LeftId { get; set; }
    public Guid RightId { get; set; }

    #region Navigation Properties

    public NghiemThu? NghiemThu { get; set; }
    public PhuLucHopDong? PhuLucHopDong { get; set; }

    #endregion
}
