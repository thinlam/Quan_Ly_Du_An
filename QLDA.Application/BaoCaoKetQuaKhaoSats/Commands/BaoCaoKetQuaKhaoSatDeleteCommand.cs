using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

public record BaoCaoKetQuaKhaoSatDeleteCommand(Guid Id) : IRequest;

internal class BaoCaoKetQuaKhaoSatDeleteCommandHandler : IRequestHandler<BaoCaoKetQuaKhaoSatDeleteCommand>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoKetQuaKhaoSatDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task Handle(BaoCaoKetQuaKhaoSatDeleteCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}
