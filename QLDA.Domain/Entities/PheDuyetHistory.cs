using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Lịch sử phê duyệt dùng chung (polymorphic) — thay thế per-entity history tables
/// </summary>
public class PheDuyetHistory : Entity<Guid>, IAggregateRoot {
    public string EntityName { get; set; } = default!;
    public Guid EntityId { get; set; }
    public Guid DuAnId { get; set; }

    /// <summary>
    /// USER_MASTER.UserPortalId
    /// </summary>
    public long? NguoiXuLyId { get; set; }

    public int? TrangThaiId { get; set; }
    public string? NoiDung { get; set; }
    public DateTimeOffset NgayXuLy { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }

    #endregion
}
