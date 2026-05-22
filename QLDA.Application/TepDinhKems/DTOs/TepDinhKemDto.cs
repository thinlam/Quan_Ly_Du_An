using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.TepDinhKems.DTOs;

public class TepDinhKemDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveCreated, IMayHaveUpdate {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() => Id ?? SequentialGuid.SequentialGuidGenerator.Instance.NewGuid();

    [DefaultValue("Guid")] public string? GroupId { get; set; }
    [DefaultValue(null)] public string? GroupType { get; set; }

    /// <summary>
    /// Loại tệp
    /// </summary>
    [DefaultValue(null)]
    public string? Type { get; set; }

    /// <summary>
    /// Tên tệp mới
    /// </summary>
    [DefaultValue(null)]
    public string? FileName { get; set; }

    /// <summary>
    /// Tên tệp gốc
    /// </summary>
    [DefaultValue(null)]
    public string? OriginalName { get; set; }

    /// <summary>
    /// Đường dẫn lưu tệp
    /// </summary>
    [DefaultValue(null)]
    public string? Path { get; set; }

    /// <summary>
    /// Kích thước
    /// </summary>
    public long Size { get; set; }

    #region Ký số

    public Guid? ParentId { get; set; } //ban đầu sẽ có 1 file gốc, sau đó đem file này đi ký số sẽ tạo ra file child

    /// <summary>Người tạo — join UserMaster.HoTen qua CreatedBy = UserPortalId (list ký số).</summary>
    public string? TenNguoiTao { get; set; }

    #endregion

    [System.Text.Json.Serialization.JsonIgnoreAttribute] public string CreatedBy { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnoreAttribute] public DateTimeOffset CreatedAt { get; set; }
    [System.Text.Json.Serialization.JsonIgnoreAttribute] public string UpdatedBy { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnoreAttribute] public DateTimeOffset? UpdatedAt { get; set; }
}