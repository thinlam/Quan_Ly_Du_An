using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.DeXuatChuyenTieps.Commands;

public record DeXuatChuyenTiepDeleteCommand(Guid Id) : IRequest<int>
{
}

public record DeXuatChuyenTiepDeleteCommandHandler : IRequestHandler<DeXuatChuyenTiepDeleteCommand, int>
{
    private readonly IRepository<DeXuatChuyenTiep, Guid> DeXuatChuyenTiep;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatChuyenTiepDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        DeXuatChuyenTiep = serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = DeXuatChuyenTiep.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatChuyenTiepDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await DeXuatChuyenTiep.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}