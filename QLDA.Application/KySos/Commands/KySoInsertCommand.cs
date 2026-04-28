using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KySos.DTOs;

namespace QLDA.Application.KySos.Commands;

public record KySoInsertCommand(KySoInsertDto Dto) : IRequest<KySo>;

internal class KySoInsertCommandHandler : IRequestHandler<KySoInsertCommand, KySo> {
    private readonly IRepository<KySo, Guid> KySo;
    private readonly IUnitOfWork _unitOfWork;

    public KySoInsertCommandHandler(IServiceProvider serviceProvider) {
        KySo = serviceProvider.GetRequiredService<IRepository<KySo, Guid>>();
        _unitOfWork = KySo.UnitOfWork;
    }

    public async Task<KySo> Handle(KySoInsertCommand request, CancellationToken cancellationToken = default) {
        var entity = request.Dto.ToEntity();

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await KySo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}