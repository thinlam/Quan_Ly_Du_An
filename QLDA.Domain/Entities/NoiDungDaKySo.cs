namespace QLDA.Domain.Entities;

/// <summary>
/// Lịch sử nội dung đã ký số
/// </summary>
public class NoiDungDaKySo : Entity<Guid>, IAggregateRoot {
    /// <summary>
    /// FK → TepDinhKem – tệp đã ký (ParentId != null) vừa được insert ở Bước 1
    /// </summary>
    public Guid TepDinhKemId { get; set; }

    /// <summary>
    /// Tên tệp đã lưu (từ TepDinhKem.FileName)
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Tên tệp gốc (từ TepDinhKem.OriginalName)
    /// </summary>
    public string? FileOrginal { get; set; }

    /// <summary>
    /// Id đối tượng chủ (từ TepDinhKem.GroupId)
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Tên loại đối tượng chủ (từ TepDinhKem.GroupType)
    /// </summary>
    public string? GroupName { get; set; }

    // CreateUserId → CreatedBy (kế thừa từ Entity<Guid>, auto-set bởi AuditInterceptor)
    // CreateDate   → CreatedAt shadow property (auto-set bởi ConfigureForBase)

    #region Navigation Properties
    public TepDinhKem? TepDinhKem { get; set; }
    #endregion
}