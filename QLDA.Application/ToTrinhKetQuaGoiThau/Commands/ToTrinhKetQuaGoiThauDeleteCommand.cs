using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.Commands;

public record ToTrinhKetQuaGoiThauDeleteCommand(Guid Id) : IRequest<int>
{
}

public record ToTrinhKetQuaGoiThauDeleteCommandHandler : IRequestHandler<ToTrinhKetQuaGoiThauDeleteCommand, int>
{
    private readonly IRepository<ToTrinhKetQuaGoiThau, Guid> ToTrinhKetQuaGoiThau;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhKetQuaGoiThauDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        ToTrinhKetQuaGoiThau = serviceProvider.GetRequiredService<IRepository<ToTrinhKetQuaGoiThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = ToTrinhKetQuaGoiThau.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhKetQuaGoiThauDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await ToTrinhKetQuaGoiThau.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}