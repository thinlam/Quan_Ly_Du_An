using Microsoft.EntityFrameworkCore;
namespace BuildingBlocks.Application.Common;

public static class SyncHelper {

    /// <summary>
    /// Có 5 trường hợp cần quan tâm: <br/>
    /// - <b>Delete All</b>: Khi request rỗng, xóa mềm tất cả entities hiện tại => <b>requestEntities null hoặc empty</b><br/>
    /// - <b>Add</b>:  Khi entity không tồn tại trong database => <b> Id không tồn tại trong db </b> <br/>
    /// - <b>Restore</b>:  Khi entity bị xóa mềm (IsDeleted = true), sync sẽ khôi phục (IsDeleted = false) và cập nhật => <b> Id tồn tại trong db và đã bị xoá tạm</b><br/>
    /// - <b>Update</b>:  Khi entity tồn tại và chưa bị xóa, sync sẽ cập nhật => <b> </b> Id tồn tại trong db và chưa bị xoá <br/>
    /// - <b>Soft Delete</b>:  Khi entity tồn tại chưa bị xóa nhưng không có trong request, sync sẽ xóa mềm (IsDeleted = true) => <b>Id trong db không có trong request </b><br/>
    /// </summary>
    public static async Task SyncCollection<T, TKey>(
        IRepository<T, TKey> repository,
        ICollection<T>? existingEntities,            // collection tracked bởi DbContext (navigation hoặc từ DbSet)
        IEnumerable<T>? requestEntities,             // từ DTO -> entity (chưa tracked)
        Action<T, T>? updateAction = null,
        CancellationToken cancellationToken = default
    )
    where T : class, IHasKey<TKey>, IMayHaveDelete, IAggregateRoot, new()
    where TKey : notnull {
        await SyncCollection(repository, existingEntities, requestEntities, updateAction, disableAudit: false, hardDelete: false, cancellationToken);
    }

    /// <summary>
    /// SyncCollection với tùy chọn disable audit và hard delete cho các entities
    /// </summary>
    public static async Task SyncCollection<T, TKey>(
        IRepository<T, TKey> repository,
        ICollection<T>? existingEntities,            // collection tracked bởi DbContext (navigation hoặc từ DbSet)
        IEnumerable<T>? requestEntities,             // từ DTO -> entity (chưa tracked)
        Action<T, T>? updateAction = null,
        bool disableAudit = false,                   // true: disable audit cho entities được tạo mới/cập nhật
        bool hardDelete = false,                     // true: xóa vĩnh viễn thay vì soft delete
        CancellationToken cancellationToken = default
    )
    where T : class, IHasKey<TKey>, IMayHaveDelete, IAggregateRoot, new()
    where TKey : notnull {
        if (hardDelete) {
            if (repository.UnitOfWork.HasTransaction) {
                await SyncCollectionHardDelete(repository, existingEntities, requestEntities, updateAction, disableAudit, cancellationToken);
            } else {
                using var tx = await repository.UnitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
                await SyncCollectionHardDelete(repository, existingEntities, requestEntities, updateAction, disableAudit, cancellationToken);
                await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
                await repository.UnitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } else {
            await SyncCollectionSoftDelete(repository, existingEntities, requestEntities, updateAction, disableAudit, cancellationToken);
        }
    }

    /// <summary>
    /// SyncCollection với Soft Delete - chỉ đánh dấu IsDeleted = true
    /// Flow: ADD → UPDATE → SOFT DELETE (thứ tự không quan trọng vì không xóa thật)
    /// </summary>
    private static async Task SyncCollectionSoftDelete<T, TKey>(
        IRepository<T, TKey> repository,
        ICollection<T>? existingEntities,
        IEnumerable<T>? requestEntities,
        Action<T, T>? updateAction,
        bool disableAudit,
        CancellationToken cancellationToken
    )
    where T : class, IHasKey<TKey>, IMayHaveDelete, IAggregateRoot, new()
    where TKey : notnull {
        List<T> existingList = [.. existingEntities ?? []];

        // Nếu request rỗng, soft delete tất cả
        if (requestEntities == null || !requestEntities.Any()) {
            foreach (T entity in existingList.Where(e => !e.IsDeleted)) {
                if (disableAudit && entity is IConditionalAuditable conditionalAuditable)
                    conditionalAuditable.DisableAudit();
                entity.IsDeleted = true;
            }
            return;
        }

        Dictionary<TKey, T> existingMap = existingList.ToDictionary(e => e.Id, e => e);
        HashSet<TKey> requestIds = [.. requestEntities.Select(r => r.Id)];

        List<T> toAdd = [.. requestEntities.Where(r => !existingMap.ContainsKey(r.Id))];
        List<T> toRestore = [.. requestEntities.Where(r => existingMap.TryGetValue(r.Id, out T? ex) && ex.IsDeleted)];
        List<T> toUpdate = [.. requestEntities.Where(r => existingMap.TryGetValue(r.Id, out T? ex) && !ex.IsDeleted)];
        List<T> toSoftDelete = [.. existingList.Where(e => !e.IsDeleted && !requestIds.Contains(e.Id))];

        // ADD
        if (toAdd.Count > 0) {
            if (disableAudit)
                foreach (var entity in toAdd)
                    if (entity is IConditionalAuditable conditionalAuditable)
                        conditionalAuditable.DisableAudit();

            await repository.AddRangeAsync(toAdd, cancellationToken);
        }

        // RESTORE & UPDATE
        if (updateAction != null) {
            foreach (T model in toRestore) {
                T existing = existingMap[model.Id];
                existing.IsDeleted = false;
                if (disableAudit && existing is IConditionalAuditable conditionalAuditable)
                    conditionalAuditable.DisableAudit();
                updateAction(existing, model);
            }

            foreach (T model in toUpdate) {
                T existing = existingMap[model.Id];
                if (disableAudit && existing is IConditionalAuditable conditionalAuditable)
                    conditionalAuditable.DisableAudit();
                updateAction(existing, model);
            }
        }

        // SOFT DELETE
        foreach (T entity in toSoftDelete) {
            if (disableAudit && entity is IConditionalAuditable conditionalAuditable)
                conditionalAuditable.DisableAudit();
            entity.IsDeleted = true;
        }
    }

    /// <summary>
    /// SyncCollection với Hard Delete - xóa vĩnh viễn khỏi database
    /// Flow: HARD DELETE → SaveChanges → UPDATE → ADD
    /// (Phải flush DELETE trước ADD để tránh unique constraint violation)
    /// </summary>
    private static async Task SyncCollectionHardDelete<T, TKey>(
        IRepository<T, TKey> repository,
        ICollection<T>? existingEntities,
        IEnumerable<T>? requestEntities,
        Action<T, T>? updateAction,
        bool disableAudit,
        CancellationToken cancellationToken
    )
    where T : class, IHasKey<TKey>, IMayHaveDelete, IAggregateRoot, new()
    where TKey : notnull {
        List<T> existingList = [.. existingEntities ?? []];

        // Nếu request rỗng, hard delete tất cả
        if (requestEntities == null || !requestEntities.Any()) {
            foreach (T entity in existingList) {
                if (disableAudit && entity is IConditionalAuditable conditionalAuditable)
                    conditionalAuditable.DisableAudit();
                repository.Delete(entity);
                existingEntities?.Remove(entity);
            }
            return;
        }

        Dictionary<TKey, T> existingMap = existingList.ToDictionary(e => e.Id, e => e);
        HashSet<TKey> requestIds = [.. requestEntities.Select(r => r.Id)];

        List<T> toAdd = [.. requestEntities.Where(r => !existingMap.ContainsKey(r.Id))];
        List<T> toUpdate = [.. requestEntities.Where(r => existingMap.ContainsKey(r.Id))];
        List<T> toHardDelete = [.. existingList.Where(e => !requestIds.Contains(e.Id))];

        // HARD DELETE - phải thực hiện và flush trước khi ADD
        if (toHardDelete.Count > 0) {
            foreach (T entity in toHardDelete) {
                if (disableAudit && entity is IConditionalAuditable conditionalAuditable)
                    conditionalAuditable.DisableAudit();
                repository.Delete(entity);
                existingEntities?.Remove(entity);
            }

            // Flush DELETE to database trước khi ADD để tránh unique constraint violation
            if (toAdd.Count > 0) {
                await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        // UPDATE - không cần phân biệt restore/update vì hard delete không dùng IsDeleted
        if (updateAction != null) {
            foreach (T model in toUpdate) {
                T existing = existingMap[model.Id];
                if (disableAudit && existing is IConditionalAuditable conditionalAuditable)
                    conditionalAuditable.DisableAudit();
                updateAction(existing, model);
            }
        }

        // ADD - sau khi đã xóa và flush
        if (toAdd.Count > 0) {
            if (disableAudit)
                foreach (var entity in toAdd)
                    if (entity is IConditionalAuditable conditionalAuditable)
                        conditionalAuditable.DisableAudit();

            await repository.AddRangeAsync(toAdd, cancellationToken);
        }
    }

    /// <summary>
    /// Sets IsDeleted = true on Attachment entities by groupIds.
    /// </summary>
    public static async Task SetDeleteWithRelatedFiles(
        IRepository<Attachment, Guid> attachmentRepository,
        List<string> groupIds,
        CancellationToken cancellationToken = default
    ) => await SetDeleteWithRelatedFiles(attachmentRepository, groupIds, false, cancellationToken);

    /// <summary>
    /// Sets IsDeleted = true on Attachment entities by groupIds.
    /// </summary>
    public static async Task SetDeleteWithRelatedFiles(
        IRepository<Attachment, Guid> attachmentRepository,
        List<string> groupIds,
        bool disableAudit,
        CancellationToken cancellationToken = default
    ) {
        if (groupIds == null || groupIds.Count == 0)
            return;

        var files = await attachmentRepository.GetOrderedSet()
            .Where(o => groupIds.Contains(o.GroupId)).ToListAsync(cancellationToken);

        foreach (var file in files) {
            if (disableAudit && file is IConditionalAuditable conditionalAuditable) {
                conditionalAuditable.DisableAudit();
            }
            file.IsDeleted = true;
        }
    }
}
