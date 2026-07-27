using BuildingBlocks.Application.Attachments.Common;

namespace BuildingBlocks.Application.Attachments.DTOs;

public static class AttachmentMapping
{
    public static List<AttachmentDto> ToListDto(this IEnumerable<Attachment> danhSachAttachment)
        => [.. danhSachAttachment.Select(o => o.ToDto())];

    public static AttachmentDto ToDto(this Attachment entity)
        => entity.ToDto<AttachmentDto>();

    /// <summary>
    /// Map entity → DTO generic (T implement IAttachmentDto, có constructor không tham số).
    /// Chỉ map 9 core fields — field module-specific do caller tự set.
    /// </summary>
    public static T ToDto<T>(this Attachment entity) where T : IAttachmentDto, new()
        => new()
        {
            Id = entity.Id,
            ParentId = entity.ParentId,
            GroupId = entity.GroupId,
            GroupType = entity.GroupType,
            Type = entity.Type,
            FileName = entity.FileName,
            OriginalName = entity.OriginalName,
            Path = entity.Path,
            Size = entity.Size,
        };

    private static Attachment ToEntity(this AttachmentInsertModel model, string groupId, string baseGroupType)
        => new()
        {
            Id = GuidExtensions.GetSequentialGuidId(),
            ParentId = model.ParentId,
            GroupId = groupId,
            GroupType = baseGroupType.ResolveSignedGroupType(model.ParentId != null),
            Type = model.Type,
            FileName = model.FileName,
            OriginalName = model.OriginalName,
            Path = model.Path,
            Size = model.Size,
        };

    private static Attachment ToEntity(this AttachmentInsertOrUpdateModel model, string groupId, string baseGroupType)
        => new()
        {
            Id = model.Id.GetId(),
            ParentId = model.ParentId,
            GroupId = groupId,
            GroupType = baseGroupType.ResolveSignedGroupType(model.ParentId != null),
            Type = model.Type,
            FileName = model.FileName,
            OriginalName = model.OriginalName,
            Path = model.Path,
            Size = model.Size,
        };

    /// <summary>
    /// Chuyển đổi danh sách AttachmentInsertModel thành entities.
    /// baseGroupType bắt buộc; GroupType được resolve theo ParentId (ký số).
    /// </summary>
    public static IEnumerable<Attachment> ToEntities(
        this List<AttachmentInsertModel> data,
        string groupId,
        string baseGroupType)
    {
        if (string.IsNullOrWhiteSpace(baseGroupType))
            throw new ArgumentException("baseGroupType là bắt buộc.", nameof(baseGroupType));

        if (long.TryParse(groupId, out _) && string.IsNullOrEmpty(baseGroupType))
            throw new ArgumentException("Bắt buộc phải truyền 'baseGroupType' khi 'groupId' là số.", nameof(baseGroupType));

        return data.Select(m => ToEntity(m, groupId, baseGroupType));
    }

    /// <summary>
    /// Chuyển đổi danh sách AttachmentInsertOrUpdateModel thành entities.
    /// baseGroupType bắt buộc; GroupType được resolve theo ParentId (ký số).
    /// </summary>
    public static IEnumerable<Attachment> ToEntities(
        this List<AttachmentInsertOrUpdateModel> data,
        string groupId,
        string baseGroupType)
    {
        if (string.IsNullOrWhiteSpace(baseGroupType))
            throw new ArgumentException("baseGroupType là bắt buộc.", nameof(baseGroupType));

        return data.Select(m => ToEntity(m, groupId, baseGroupType));
    }
}
