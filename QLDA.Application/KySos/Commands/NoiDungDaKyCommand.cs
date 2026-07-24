using System.Data;
using BuildingBlocks.Application.Attachments.Common;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Enums;

namespace QLDA.Application.KySos.Commands;

public record NoiDungDaKyCommand : IRequest<int> {
    public required string GroupId { get; set; }
    public required List<Attachment> Entities { get; set; }
}

internal class NoiDungDaKyCommandHandler : IRequestHandler<NoiDungDaKyCommand, int> {
    private readonly IRepository<Attachment, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public NoiDungDaKyCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(NoiDungDaKyCommand request, CancellationToken cancellationToken = default) {
        var toInsert = new List<Attachment>();

        foreach (var entity in request.Entities) {
            entity.GroupId = request.GroupId;
            EnsureSignedGroupType(entity);

            // ParentId có nhưng chưa có bản ghi cha (ký trước khi lưu form) → vẫn lưu file ký số.
            // ParentId null → cũng lưu, coi như file ký số độc lập.
            if (entity.ParentId is { } parentId) {
                var parentExists = await _repository.GetQueryableSet()
                    .AnyAsync(e => e.Id == parentId, cancellationToken);
                if (!parentExists)
                    entity.ParentId = null;
            }

            toInsert.Add(entity);
        }

        if (toInsert.Count == 0)
            return 0;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddRangeAsync(toInsert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return toInsert.Count;
    }

    /// <summary>
    /// API ký số luôn lưu bản ghi là file ký số (GroupType chứa KySo), kể cả khi không có ParentId.
    /// </summary>
    private static void EnsureSignedGroupType(Attachment entity) {
        if (string.IsNullOrWhiteSpace(entity.GroupType)) {
            entity.GroupType = nameof(EGroupType.KySo);
            return;
        }

        if (entity.GroupType.Contains("KySo", StringComparison.Ordinal))
            return;

        entity.GroupType = SignedGroupTypeHelper.WithSignedVariant(entity.GroupType);
    }
}
