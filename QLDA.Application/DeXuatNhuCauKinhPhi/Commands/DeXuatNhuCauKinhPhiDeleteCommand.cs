using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Commands;

public record DeXuatNhuCauKinhPhiDeleteCommand(Guid Id) : IRequest<int>
{
}

public record DeXuatNhuCauKinhPhiDeleteCommandHandler : IRequestHandler<DeXuatNhuCauKinhPhiDeleteCommand, int>
{
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> DeXuatNhuCauKinhPhi;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatNhuCauKinhPhiDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        DeXuatNhuCauKinhPhi = serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = DeXuatNhuCauKinhPhi.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatNhuCauKinhPhiDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await DeXuatNhuCauKinhPhi.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}