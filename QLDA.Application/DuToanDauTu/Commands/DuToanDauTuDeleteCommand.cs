using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Entities;

namespace QLDA.Application.DuToanDauTus.Commands;

public record DuToanDauTuDeleteCommand(Guid Id) : IRequest<int>
{
}

public record DuToanDauTuDeleteCommandHandler : IRequestHandler<DuToanDauTuDeleteCommand, int>
{
    private readonly IRepository<DuToanDauTu, Guid> DuToanDauTu;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public DuToanDauTuDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        DuToanDauTu = serviceProvider.GetRequiredService<IRepository<DuToanDauTu, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = DuToanDauTu.UnitOfWork;
    }

    public async Task<int> Handle(DuToanDauTuDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await DuToanDauTu.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}