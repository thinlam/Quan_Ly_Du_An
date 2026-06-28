using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

public class VanBanQuyetDinh : Entity<Guid>, IAggregateRoot, ITienDo, IVanBanQuyetDinh, INguoiKy, IEntityType {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? Loai { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DuAnBuoc? DuAnBuoc { get; set; }
    #endregion
}