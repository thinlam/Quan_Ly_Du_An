using System.Data;
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.KySos.Commands;

public record KySoDeleteCommand(Guid Id) : IRequest;

internal class KySoDeleteCommandHandler : IRequestHandler<KySoDeleteCommand> {
    private readonly IRepository<KySo, Guid> KySo;
    private readonly IUnitOfWork _unitOfWork;

    public KySoDeleteCommandHandler(IServiceProvider serviceProvider) {
        KySo = serviceProvider.GetRequiredService<IRepository<KySo, Guid>>();
        _unitOfWork = KySo.UnitOfWork;
    }

    public async Task Handle(KySoDeleteCommand request, CancellationToken cancellationToken = default) {
        var entity = await KySo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await KySo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}