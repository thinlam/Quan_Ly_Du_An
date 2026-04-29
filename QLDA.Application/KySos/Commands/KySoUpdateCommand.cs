using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KySos.DTOs;

namespace QLDA.Application.KySos.Commands;

public record KySoUpdateCommand(KySoUpdateModel Model) : IRequest<KySo>;

internal class KySoUpdateCommandHandler : IRequestHandler<KySoUpdateCommand, KySo> {
    private readonly IRepository<KySo, Guid> _kySo;
    private readonly IUnitOfWork _unitOfWork;

    public KySoUpdateCommandHandler(IServiceProvider serviceProvider) {
        _kySo = serviceProvider.GetRequiredService<IRepository<KySo, Guid>>();
        _unitOfWork = _kySo.UnitOfWork;
    }

    public async Task<KySo> Handle(KySoUpdateCommand request, CancellationToken cancellationToken = default) {
        var entity = await _kySo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.Update(request.Model);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _kySo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}