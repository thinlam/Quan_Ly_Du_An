using BuildingBlocks.Application.Attachments.DTOs;
using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.TepDinhKems.DTOs;

/// <summary>
/// Compatibility DTO — giữ tên UI contract; core fields kế thừa từ AttachmentDto.
/// </summary>
public class TepDinhKemDto : AttachmentDto, IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveCreated, IMayHaveUpdate
{
    public Guid GetId() => Id ?? SequentialGuid.SequentialGuidGenerator.Instance.NewGuid();

    /// <summary>Người tạo — join UserMaster.HoTen qua CreatedBy = UserPortalId (list ký số).</summary>
    public string? TenNguoiTao { get; set; }

    [System.Text.Json.Serialization.JsonIgnoreAttribute] public string CreatedBy { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnoreAttribute] public DateTimeOffset CreatedAt { get; set; }
    [System.Text.Json.Serialization.JsonIgnoreAttribute] public string UpdatedBy { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnoreAttribute] public DateTimeOffset? UpdatedAt { get; set; }
}
