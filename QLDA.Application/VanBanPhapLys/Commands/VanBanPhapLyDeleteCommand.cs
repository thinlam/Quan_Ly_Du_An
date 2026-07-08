using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.VanBanPhapLys.Commands;

public record VanBanPhapLyDeleteCommand(Guid Id) : IRequest<int> {
}

public record VanBanPhapLyDeleteCommandHandler : IRequestHandler<VanBanPhapLyDeleteCommand, int> {
    private readonly IRepository<VanBanPhapLy, Guid> VanBanPhapLy;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public VanBanPhapLyDeleteCommandHandler(IServiceProvider serviceProvider) {
        VanBanPhapLy = serviceProvider.GetRequiredService<IRepository<VanBanPhapLy, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = VanBanPhapLy.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<int> Handle(VanBanPhapLyDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await VanBanPhapLy.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}