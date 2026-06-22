using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Entities;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Commands;

public record DeXuatNhuCauKinhPhiDeleteCommand(Guid Id) : IRequest<int>
{
}

public record DeXuatNhuCauKinhPhiDeleteCommandHandler : IRequestHandler<DeXuatNhuCauKinhPhiDeleteCommand, int>
{
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> DeXuatNhuCauKinhPhi;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatNhuCauKinhPhiDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        DeXuatNhuCauKinhPhi = serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = DeXuatNhuCauKinhPhi.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatNhuCauKinhPhiDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await DeXuatNhuCauKinhPhi.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}