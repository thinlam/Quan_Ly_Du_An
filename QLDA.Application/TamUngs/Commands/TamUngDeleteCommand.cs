using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.TamUngs.Commands;

public record TamUngDeleteCommand(Guid Id) : IRequest;
public record TamUngDeleteCommandHandler : IRequestHandler<TamUngDeleteCommand> {
    private readonly IRepository<TamUng, Guid> TamUng;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public TamUngDeleteCommandHandler(IServiceProvider serviceProvider) {
        TamUng = serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = TamUng.UnitOfWork;
    }

    public async Task Handle(TamUngDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await TamUng.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);
        
        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }
}