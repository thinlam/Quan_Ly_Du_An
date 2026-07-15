namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class ThanhLyHopDongNghiemThu : IJunctionEntity<Guid, Guid>, IAggregateRoot
{
    public Guid LeftId { get; set; }
    public Guid RightId { get; set; }

    #region Navigation Properties

    public ThanhLyHopDong? ThanhLy { get; set; }
    public NghiemThu? NghiemThu { get; set; }
    #endregion
}