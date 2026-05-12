using System.Data;
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

public record HoSoMoiThauDienTuDeleteCommand(Guid Id) : IRequest;

internal class HoSoMoiThauDienTuDeleteCommandHandler : IRequestHandler<HoSoMoiThauDienTuDeleteCommand> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuDeleteCommandHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _unitOfWork = HoSoMoiThauDienTu.UnitOfWork;
    }

    public async Task Handle(HoSoMoiThauDienTuDeleteCommand request, CancellationToken cancellationToken = default) {
        var entity = await HoSoMoiThauDienTu.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoMoiThauDienTu.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}