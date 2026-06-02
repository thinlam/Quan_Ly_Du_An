using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.DuToanDauTus.Commands;

public record DuToanDauTuDeleteCommand(Guid Id) : IRequest<int>
{
}

public record DuToanDauTuDeleteCommandHandler : IRequestHandler<DuToanDauTuDeleteCommand, int>
{
    private readonly IRepository<DuToanDauTu, Guid> DuToanDauTu;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public DuToanDauTuDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        DuToanDauTu = serviceProvider.GetRequiredService<IRepository<DuToanDauTu, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = DuToanDauTu.UnitOfWork;
    }

    public async Task<int> Handle(DuToanDauTuDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await DuToanDauTu.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}