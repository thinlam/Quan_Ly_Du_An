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

            if (entity.ParentId is { } parentId) {
                var parent = await _repository.GetQueryableSet()
                    .FirstOrDefaultAsync(e => e.Id == parentId, cancellationToken);

                if (parent is null) {
                    // Ký trước khi lưu form / parent không tồn tại — cần GroupType từ caller.
                    entity.ParentId = null;
                    RequireGroupTypeWhenNoParent(entity);
                    EnsureSignedGroupType(entity);
                }
                else {
                    // Derive KySo_<base> từ parent (vd. BanGiaoHoSo → KySo_BanGiaoHoSo).
                    // Tránh KySo_KySo khi caller hard-code EGroupType.KySo vào ToEntities.
                    ApplySignedGroupTypeFromParent(entity, parent);
                }
            }
            else {
                RequireGroupTypeWhenNoParent(entity);
                EnsureSignedGroupType(entity);
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
    /// GroupType ký số = KySo_&lt;base của parent&gt;. Parent đã là KySo_* thì strip về base trước.
    /// </summary>
    private static void ApplySignedGroupTypeFromParent(Attachment entity, Attachment parent) {
        var baseType = parent.GroupType.ToBaseGroupType() ?? parent.GroupType;

        if (string.IsNullOrWhiteSpace(baseType) || baseType == nameof(EGroupType.KySo)) {
            EnsureSignedGroupType(entity);
            return;
        }

        entity.GroupType = SignedGroupTypeHelper.WithSignedVariant(baseType);
    }

    /// <summary>
    /// Ký trực tiếp (không có ParentId hợp lệ): caller phải truyền GroupType base hoặc KySo_&lt;base&gt;.
    /// </summary>
    private static void RequireGroupTypeWhenNoParent(Attachment entity) {
        if (!IsMissingGroupTypeForDirectSign(entity.GroupType))
            return;

        ManagedException.Throw(
            "Khi không có ParentId, bắt buộc truyền GroupType (vd. BanGiaoHoSo hoặc KySo_BanGiaoHoSo).");
    }

    private static bool IsMissingGroupTypeForDirectSign(string? groupType) {
        if (string.IsNullOrWhiteSpace(groupType))
            return true;

        var trimmed = groupType.Trim();
        if (trimmed == nameof(EGroupType.None) || trimmed == nameof(EGroupType.KySo))
            return true;

        if (trimmed == $"{SignedGroupTypeHelper.Prefix}{nameof(EGroupType.KySo)}"
            || trimmed == $"{SignedGroupTypeHelper.Prefix}{nameof(EGroupType.None)}")
            return true;

        var baseType = trimmed.ToBaseGroupType() ?? trimmed;
        return baseType == nameof(EGroupType.None) || baseType == nameof(EGroupType.KySo);
    }

    /// <summary>
    /// Khi không có parent hợp lệ: đảm bảo GroupType chứa KySo (fallback sentinel <c>KySo</c>).
    /// Chuẩn hóa <c>KySo_KySo</c> → <c>KySo</c>.
    /// </summary>
    private static void EnsureSignedGroupType(Attachment entity) {
        if (string.IsNullOrWhiteSpace(entity.GroupType)
            || entity.GroupType == nameof(EGroupType.KySo)
            || entity.GroupType == $"{SignedGroupTypeHelper.Prefix}{nameof(EGroupType.KySo)}") {
            entity.GroupType = nameof(EGroupType.KySo);
            return;
        }

        if (entity.GroupType.StartsWith(SignedGroupTypeHelper.Prefix, StringComparison.Ordinal))
            return;

        entity.GroupType = SignedGroupTypeHelper.WithSignedVariant(entity.GroupType);
    }
}
