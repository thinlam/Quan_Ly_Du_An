using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Commands;

public record HoSoDeXuatCapDoCnttDeleteCommand(Guid Id) : IRequest;

internal class HoSoDeXuatCapDoCnttDeleteCommandHandler : IRequestHandler<HoSoDeXuatCapDoCnttDeleteCommand> {
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> HoSoDeXuatCapDoCntt;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoDeXuatCapDoCnttDeleteCommandHandler(IServiceProvider serviceProvider) {
        HoSoDeXuatCapDoCntt = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _unitOfWork = HoSoDeXuatCapDoCntt.UnitOfWork;
    }

    public async Task Handle(HoSoDeXuatCapDoCnttDeleteCommand request, CancellationToken cancellationToken = default) {
        var entity = await HoSoDeXuatCapDoCntt.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;
        
        // Soft delete file đính kèm
        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, new List<string> { entity.Id.ToString() }, cancellationToken);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoDeXuatCapDoCntt.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}