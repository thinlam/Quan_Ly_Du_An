using System.Data;
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.KySos.Commands;

public record NoiDungDaKyCommand : IRequest<int> {
    public required string GroupId { get; set; }
    public required List<TepDinhKem> Entities { get; set; }
}

internal class NoiDungDaKyCommandHandler : IRequestHandler<NoiDungDaKyCommand, int> {
    private readonly IRepository<TepDinhKem, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public NoiDungDaKyCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(NoiDungDaKyCommand request, CancellationToken cancellationToken = default) {
        var toInsert = new List<TepDinhKem>();

        foreach (var entity in request.Entities.Where(e => e.ParentId != null)) {
            entity.GroupId = request.GroupId;

            var parent = await _repository.GetQueryableSet()
                .FirstOrDefaultAsync(e => e.Id == entity.ParentId, cancellationToken);
            ManagedException.ThrowIfNull(parent, "Không tìm thấy tệp cha (ParentId)");

            //if (IsSignedVersion(parent.GroupType)) {
            //    entity.ParentId = parent.Id;
            //} else {
            //    entity.GroupType = GroupTypeConstants.KySo;
            //}

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

}
