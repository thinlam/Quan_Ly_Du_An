using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.ThuyetMinhDuAns.Commands;

public record ThuyetMinhDuAnDeleteCommand(Guid Id) : IRequest<int>
{
}

public record ThuyetMinhDuAnDeleteCommandHandler : IRequestHandler<ThuyetMinhDuAnDeleteCommand, int>
{
    private readonly IRepository<ThuyetMinhDuAn, Guid> ThuyetMinhDuAn;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ThuyetMinhDuAnDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        ThuyetMinhDuAn = serviceProvider.GetRequiredService<IRepository<ThuyetMinhDuAn, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = ThuyetMinhDuAn.UnitOfWork;
    }

    public async Task<int> Handle(ThuyetMinhDuAnDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await ThuyetMinhDuAn.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}