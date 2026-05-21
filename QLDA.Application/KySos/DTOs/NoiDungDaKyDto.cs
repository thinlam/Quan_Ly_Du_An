namespace QLDA.Application.KySos.DTOs;

/// <summary>
/// File đã ký — projection từ TepDinhKem (ParentId != null).
/// </summary>
public class NoiDungDaKyDto {
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string? FileName { get; set; }

    public string? FileOrginal { get; set; }

    public string? GroupId { get; set; }

    /// <summary>Loại đối tượng — từ TepDinhKem.GroupType.</summary>
    public string? GroupName { get; set; }

    public long? CreateUserId { get; set; }

    public string? CreateUserName { get; set; }

    public DateOnly? CreateDate { get; set; }
}
