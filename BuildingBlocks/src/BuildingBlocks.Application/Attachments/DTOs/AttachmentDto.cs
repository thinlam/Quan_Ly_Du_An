using BuildingBlocks.Application.Attachments.Common;

namespace BuildingBlocks.Application.Attachments.DTOs;

/// <summary>
/// DTO core 9 fields — KHÔNG thêm field module-specific (vd TenNguoiTao).
/// Module cần field riêng → tạo class extends AttachmentDto trong module đó.
/// </summary>
public class AttachmentDto : IAttachmentDto
{
    public Guid? Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? GroupId { get; set; }
    public string? GroupType { get; set; }

    /// <summary>Loại tệp</summary>
    public string? Type { get; set; }

    /// <summary>Tên tệp mới</summary>
    public string? FileName { get; set; }

    /// <summary>Tên tệp gốc</summary>
    public string? OriginalName { get; set; }

    /// <summary>Đường dẫn lưu tệp</summary>
    public string? Path { get; set; }

    /// <summary>Kích thước</summary>
    public long Size { get; set; }
}
