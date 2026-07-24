using BuildingBlocks.Application.Attachments.Common;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.Attachments.Commands;

/// <summary>
/// Bulk insert/update attachments trong phạm vi GroupId + GroupTypes.
/// AutoDeleteMissing=false mặc định — chỉ insert/update, không xóa file thiếu trong list.
/// </summary>
public record AttachmentBulkInsertOrUpdateCommand : IRequest
{
    /// <summary>GroupId bắt buộc.</summary>
    public required string GroupId { get; set; }

    /// <summary>
    /// Base GroupTypes bắt buộc — scope sync chặt (kèm KySo_ variants).
    /// Cho phép nhiều loại file trên cùng GroupId trong 1 call.
    /// </summary>
    public required List<string> GroupTypes { get; set; }

    /// <summary>Entities đã map (GroupType nên đã resolve hoặc sẽ được chuẩn hóa).</summary>
    public required List<Attachment> Entities { get; set; }

    /// <summary>
    /// false (mặc định): chỉ insert/update.
    /// true: soft-delete các file trong scope không có trong Entities.
    /// </summary>
    public bool AutoDeleteMissing { get; set; } = false;
}

internal class AttachmentBulkInsertOrUpdateCommandHandler(
    IRepository<Attachment, Guid> repository,
    IUnitOfWork unitOfWork,
    ILogger<AttachmentBulkInsertOrUpdateCommandHandler> logger)
    : IRequestHandler<AttachmentBulkInsertOrUpdateCommand>
{
    private readonly IRepository<Attachment, Guid> _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<AttachmentBulkInsertOrUpdateCommandHandler> _logger = logger;

    public async Task Handle(
        AttachmentBulkInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        NormalizeEntities(request);

        if (_unitOfWork.HasTransaction)
        {
            // Tham gia transaction hiện có — không commit
            await InsertOrUpdateAsync(request, cancellationToken);
        }
        else
        {
            using var tx = await _unitOfWork.BeginTransactionAsync(
                IsolationLevel.ReadCommitted, cancellationToken);
            await InsertOrUpdateAsync(request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }

    private static void Validate(AttachmentBulkInsertOrUpdateCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.GroupId))
            throw new ManagedException("GroupId là bắt buộc.");

        if (request.GroupTypes is null || request.GroupTypes.Count == 0
            || request.GroupTypes.All(string.IsNullOrWhiteSpace))
        {
            throw new ManagedException(
                "GroupTypes là bắt buộc để tránh xóa nhầm files khác cùng GroupId.");
        }

        request.Entities ??= [];
    }

    private static void NormalizeEntities(AttachmentBulkInsertOrUpdateCommand request)
    {
        var allowedBases = request.GroupTypes
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToList();

        var allAllowed = allowedBases
            .SelectMany(SignedGroupTypeHelper.ExpandWithSignedVariant)
            .ToHashSet();

        foreach (var entity in request.Entities)
        {
            entity.GroupId = request.GroupId;

            var entityBaseType = entity.GroupType.ToBaseGroupType() ?? entity.GroupType;

            // Nếu GroupType trống → gán base đầu tiên + resolve theo ParentId
            if (string.IsNullOrWhiteSpace(entity.GroupType))
            {
                entity.GroupType = allowedBases[0]
                    .ResolveSignedGroupType(entity.ParentId != null);
                continue;
            }

            // Re-resolve để tránh KySo_KySo_ và đồng bộ ParentId
            if (allowedBases.Contains(entityBaseType)
                || allAllowed.Contains(entity.GroupType))
            {
                var matchedBase = allowedBases.FirstOrDefault(bt =>
                    bt == entityBaseType
                    || SignedGroupTypeHelper.WithSignedVariant(bt) == entity.GroupType)
                    ?? entityBaseType;

                entity.GroupType = matchedBase
                    .ResolveSignedGroupType(entity.ParentId != null);
                continue;
            }

            throw new ManagedException(
                $"GroupType='{entity.GroupType}' không thuộc scope GroupTypes=[{string.Join(", ", allowedBases)}]");
        }
    }

    private async Task InsertOrUpdateAsync(
        AttachmentBulkInsertOrUpdateCommand request,
        CancellationToken cancellationToken)
    {
        var baseGroupTypes = request.GroupTypes
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.ToBaseGroupType() ?? t)
            .Distinct()
            .ToList();

        _logger.LogDebug(
            "Sync attachments GroupId={GroupId} GroupTypes=[{GroupTypes}] Count={Count} AutoDeleteMissing={AutoDelete}",
            request.GroupId,
            string.Join(", ", baseGroupTypes),
            request.Entities.Count,
            request.AutoDeleteMissing);

        await _repository.SyncAttachmentsAsync(
            existing: null,
            submitted: request.Entities,
            baseGroupTypes: baseGroupTypes,
            autoDeleteMissing: request.AutoDeleteMissing,
            groupId: request.GroupId,
            cancellationToken: cancellationToken);
    }
}
