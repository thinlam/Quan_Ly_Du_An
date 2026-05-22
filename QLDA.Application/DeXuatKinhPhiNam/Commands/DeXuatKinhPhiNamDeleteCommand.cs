using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.Commands;

public record DeXuatNhuCauKinhPhiNamDeleteCommand(Guid Id) : IRequest<int>
{
}

public record DeXuatNhuCauKinhPhiNamDeleteCommandHandler : IRequestHandler<DeXuatNhuCauKinhPhiNamDeleteCommand, int>
{
    private readonly IRepository<DeXuatNhuCauKinhPhiNam, Guid> DeXuatNhuCauKinhPhiNam;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatNhuCauKinhPhiNamDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        DeXuatNhuCauKinhPhiNam = serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhiNam, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = DeXuatNhuCauKinhPhiNam.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatNhuCauKinhPhiNamDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await DeXuatNhuCauKinhPhiNam.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}