namespace BuildingBlocks.Application.Attachments.DTOs;

public class AttachmentInsertOrUpdateModel : IHasKey<Guid?>
{
    public Guid? Id { get; set; }
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
    public bool KySo { get; set; }

    #endregion
}