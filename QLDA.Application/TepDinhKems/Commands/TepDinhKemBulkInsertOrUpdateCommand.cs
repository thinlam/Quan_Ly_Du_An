using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.TepDinhKems.Commands;

/// <summary>
/// Bulk insert/update file attachments for a logical group.
/// Synchronizes the persisted set of <see cref="Attachment"/> rows under <c>GroupId</c>
/// (optionally narrowed by <c>GroupType</c>) with the supplied <c>Entities</c>:
///   - Rows missing from the request are deleted (cascaded through <c>SyncCollection</c>).
///   - Rows present in both have their mutable fields overwritten from the request.
///   - New rows from the request are inserted.
/// Use this when the UI sends the full desired collection for a group rather than diffs.
/// </summary>
public record TepDinhKemBulkInsertOrUpdateCommand() : IRequest
{
    public required string GroupId { get; set; }
    public required List<Attachment> Entities { get; set; }

    /// <summary>
    /// Khi <see cref="Entities"/> rỗng, giới hạn phạm vi xóa mềm theo GroupType
    /// (tránh xóa nhầm các loại file khác cùng GroupId).
    /// </summary>
    public List<string>? ScopeGroupTypes { get; set; }
}

internal class TepDinhKemBulkInsertOrUpdateCommandHandler(IRepository<Attachment, Guid> repository, IUnitOfWork unitOfWork) : IRequestHandler<TepDinhKemBulkInsertOrUpdateCommand>
{
    private readonly IRepository<Attachment, Guid> _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(TepDinhKemBulkInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        request.Entities ??= [];

        if (_unitOfWork.HasTransaction)
        {
            await InsertOrUpdateAsync(request, cancellationToken);
        }
        else
        {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await InsertOrUpdateAsync(request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }

    private async Task InsertOrUpdateAsync(TepDinhKemBulkInsertOrUpdateCommand request, CancellationToken cancellationToken = default)
    {
        #region Build sync scope (GroupType filter)

        // Narrow the sync window to only the GroupType values actually present in the request.
        // If the request omits GroupType entirely (empty/whitespace for all items), the scope
        // is the whole group, which lets a caller wipe GroupType-typed rows by sending no
        // GroupType-tagged entities. This is intentional and matches the contract of
        // SyncCollection (full desired state for the requested scope).
        var groupTypes = request.Entities
            .Select(e => e.GroupType)
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .ToList();

        if (groupTypes.Count == 0 && request.ScopeGroupTypes?.Count > 0)
        {
            groupTypes = request.ScopeGroupTypes
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();
        }

        #endregion

        #region Load existing rows for the group

        // Read the persisted rows that fall inside the sync scope. GetQueryableSet() already
        // applies IsDeleted=false and Used=true filters, so soft-deleted rows are ignored
        // here and will not be revived by SyncCollection.
        var files = await _repository.GetQueryableSet()
            .Where(e => e.GroupId == request.GroupId)
            .WhereIf(groupTypes.Count > 0, e => groupTypes.Contains(e.GroupType))
            .ToListAsync(cancellationToken);

        #endregion

        #region Reconcile persisted set with request via SyncCollection

        // SyncCollection reconciles `existingEntities` (DB) with `requestEntities` (caller's
        // desired state) using the supplied update selector:
        //   - Entities present in DB but not in request  -> deleted (range delete via repository).
        //   - Entities present in both                    -> update selector copies mutable fields.
        //   - Entities present in request but not in DB  -> inserted via repository.Add.
        // Only fields listed in the selector are touched; identity fields (Id, GroupId,
        // GroupType) are intentionally not re-assigned to avoid breaking referential use.
        await SyncHelper.SyncCollection(repository: _repository, existingEntities: files, requestEntities: request.Entities, (existing, request) =>
        {
            existing.Type = request.Type;
            existing.FileName = request.FileName;
            existing.OriginalName = request.OriginalName;
            existing.Path = request.Path;
            existing.Size = request.Size;
        }, cancellationToken: cancellationToken);

        #endregion
    }
}