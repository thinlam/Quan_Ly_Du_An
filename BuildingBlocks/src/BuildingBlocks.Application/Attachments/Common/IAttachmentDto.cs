namespace BuildingBlocks.Application.Attachments.Common;

/// <summary>
/// Contract chung cho attachment DTO — giao tiếp qua interface để không phụ thuộc vào
/// implementation cụ thể của từng module (QLDA, QLHD, NVTT).
/// KHÔNG thêm field vào đây chỉ vì 1 module.
/// </summary>
public interface IAttachmentDto
{
    Guid? Id { get; set; }
    Guid? ParentId { get; set; }
    string? GroupId { get; set; }
    string? GroupType { get; set; }
    string? Type { get; set; }
    string? FileName { get; set; }
    string? OriginalName { get; set; }
    string? Path { get; set; }
    long Size { get; set; }
}
