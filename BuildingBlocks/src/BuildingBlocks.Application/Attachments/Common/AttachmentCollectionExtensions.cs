using BuildingBlocks.Application.Attachments.DTOs;
using BuildingBlocks.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application.Attachments.Common;

/// <summary>
/// Write-side helpers (ToEntities / Sync) và read-side BaseGroupType().
/// </summary>
public static class AttachmentCollectionExtensions
{
    /// <summary>
    /// Strip "KySo_" prefix để lấy base GroupType.
    /// </summary>
    public static string BaseGroupType(this AttachmentDto dto)
        => dto.GroupType.ToBaseGroupType() ?? string.Empty;

    /// <summary>
    /// Convert DTO list → List&lt;Attachment&gt;. Caller truyền baseGroupType;
    /// helper tự resolve GroupType theo ParentId (ký số).
    /// </summary>
    public static List<Attachment> ToEntities(
        this IEnumerable<AttachmentInsertOrUpdateModel>? dtos,
        Guid groupId,
        string baseGroupType)
    {
        if (dtos == null)
            return [];

        if (string.IsNullOrWhiteSpace(baseGroupType))
            throw new ArgumentException("baseGroupType là bắt buộc.", nameof(baseGroupType));

        var groupIdStr = groupId.ToString();
        return dtos.Select(d => new Attachment
        {
            Id = d.Id.GetId(),
            GroupId = groupIdStr,
            GroupType = SignedGroupTypeHelper.ResolveSignedGroupType(
                baseGroupType, d.ParentId != null),
            ParentId = d.ParentId,
            Type = d.Type,
            FileName = d.FileName,
            OriginalName = d.OriginalName,
            Path = d.Path,
            Size = d.Size,
        }).ToList();
    }

    /// <summary>
    /// Convert IAttachmentDto list (vd TepDinhKemDto : AttachmentDto) → entities.
    /// </summary>
    public static List<Attachment> ToEntities(
        this IEnumerable<IAttachmentDto>? dtos,
        Guid groupId,
        string baseGroupType)
    {
        if (dtos == null)
            return [];

        if (string.IsNullOrWhiteSpace(baseGroupType))
            throw new ArgumentException("baseGroupType là bắt buộc.", nameof(baseGroupType));

        var groupIdStr = groupId.ToString();
        return dtos.Select(d => new Attachment
        {
            Id = d.Id ?? GuidExtensions.GetSequentialGuidId(),
            GroupId = groupIdStr,
            GroupType = SignedGroupTypeHelper.ResolveSignedGroupType(
                baseGroupType, d.ParentId != null),
            ParentId = d.ParentId,
            Type = d.Type,
            FileName = d.FileName,
            OriginalName = d.OriginalName,
            Path = d.Path,
            Size = d.Size,
        }).ToList();
    }

    /// <summary>
    /// AttachmentDto → Attachment (read-side bridge khi GetAttachmentsQuery trả DTO
    /// nhưng ToModel vẫn nhận entity).
    /// </summary>
    public static List<Attachment> ToAttachmentEntities(this IEnumerable<AttachmentDto>? dtos)
    {
        if (dtos == null)
            return [];

        return dtos.Select(d => new Attachment
        {
            Id = d.Id ?? Guid.Empty,
            ParentId = d.ParentId,
            GroupId = d.GroupId ?? string.Empty,
            GroupType = d.GroupType ?? string.Empty,
            Type = d.Type,
            FileName = d.FileName,
            OriginalName = d.OriginalName,
            Path = d.Path,
            Size = d.Size,
        }).ToList();
    }

    /// <summary>
    /// Convert insert models → List&lt;Attachment&gt; với resolve ký số theo ParentId.
    /// </summary>
    public static List<Attachment> ToEntities(
        this IEnumerable<AttachmentInsertModel>? dtos,
        Guid groupId,
        string baseGroupType)
    {
        if (dtos == null)
            return [];

        if (string.IsNullOrWhiteSpace(baseGroupType))
            throw new ArgumentException("baseGroupType là bắt buộc.", nameof(baseGroupType));

        var groupIdStr = groupId.ToString();
        return dtos.Select(d => new Attachment
        {
            Id = GuidExtensions.GetSequentialGuidId(),
            GroupId = groupIdStr,
            GroupType = SignedGroupTypeHelper.ResolveSignedGroupType(
                baseGroupType, d.ParentId != null),
            ParentId = d.ParentId,
            Type = d.Type,
            FileName = d.FileName,
            OriginalName = d.OriginalName,
            Path = d.Path,
            Size = d.Size,
        }).ToList();
    }

    /// <summary>
    /// Sync attachments với scope chặt theo baseGroupTypes (+ KySo_ variants).
    /// Không đụng file thuộc GroupType ngoài scope.
    /// autoDeleteMissing=false (mặc định): chỉ insert/update.
    /// autoDeleteMissing=true: soft-delete file trong scope không có trong submitted.
    /// groupId bắt buộc khi submitted rỗng và existing=null.
    /// </summary>
    public static async Task SyncAttachmentsAsync(
        this IRepository<Attachment, Guid> repository,
        ICollection<Attachment>? existing,
        IEnumerable<Attachment> submitted,
        IReadOnlyList<string> baseGroupTypes,
        bool autoDeleteMissing = false,
        string? groupId = null,
        CancellationToken cancellationToken = default)
    {
        if (baseGroupTypes == null || baseGroupTypes.Count == 0)
            throw new ArgumentException("baseGroupTypes là bắt buộc.", nameof(baseGroupTypes));

        var submittedList = submitted?.ToList() ?? [];

        var allGroupTypes = baseGroupTypes
            .SelectMany(SignedGroupTypeHelper.ExpandWithSignedVariant)
            .Distinct()
            .ToHashSet();

        groupId ??= submittedList.FirstOrDefault()?.GroupId
            ?? existing?.FirstOrDefault()?.GroupId;

        if (string.IsNullOrEmpty(groupId))
        {
            if (!autoDeleteMissing && submittedList.Count == 0)
                return;

            throw new ManagedException(
                "GroupId là bắt buộc khi sync attachments (submitted/existing rỗng).");
        }

        existing ??= await repository.GetOriginalSet()
            .Where(a => a.GroupId == groupId && allGroupTypes.Contains(a.GroupType))
            .ToListAsync(cancellationToken);

        var distinctGroupIds = submittedList.Select(s => s.GroupId).Distinct().ToList();
        if (distinctGroupIds.Count > 1)
            throw new ManagedException("Tất cả entities phải cùng GroupId.");

        if (submittedList.Count > 0 && distinctGroupIds[0] != groupId)
            throw new ManagedException("Submitted GroupId không khớp.");

        foreach (var e in existing)
        {
            if (!allGroupTypes.Contains(e.GroupType))
            {
                throw new ManagedException(
                    $"Phát hiện file có GroupType='{e.GroupType}' không thuộc scope " +
                    $"[{string.Join(", ", baseGroupTypes)}]. " +
                    "Có thể là bug - kiểm tra lại việc load existing.");
            }
        }

        foreach (var e in submittedList)
        {
            if (!allGroupTypes.Contains(e.GroupType))
            {
                throw new ManagedException(
                    $"Submitted entity có GroupType='{e.GroupType}' không thuộc scope " +
                    $"[{string.Join(", ", baseGroupTypes)}]");
            }
        }

        // AutoDeleteMissing=false → thu hẹp existing về intersection với request Ids
        // để SyncCollection không soft-delete các file còn lại trong scope.
        ICollection<Attachment> existingForSync = existing;
        if (!autoDeleteMissing)
        {
            var requestIds = submittedList.Select(s => s.Id).ToHashSet();
            existingForSync = existing.Where(e => requestIds.Contains(e.Id)).ToList();
        }

        await SyncHelper.SyncCollection(
            repository,
            existingForSync,
            submittedList,
            updateAction: (entity, request) =>
            {
                entity.Type = request.Type;
                entity.FileName = request.FileName;
                entity.OriginalName = request.OriginalName;
                entity.Path = request.Path;
                entity.Size = request.Size;
                entity.ParentId = request.ParentId;

                // Re-derive GroupType khi ParentId đổi — tránh KySo_KySo_
                var baseType = request.GroupType.ToBaseGroupType() ?? request.GroupType;
                entity.GroupType = baseType.ResolveSignedGroupType(request.ParentId != null);
            },
            cancellationToken: cancellationToken);
    }
}
