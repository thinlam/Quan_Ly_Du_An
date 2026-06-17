using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;

namespace QLDA.Application.PhuLucHopDongs.Commands;

public record PhuLucHopDongDeleteCommand(Guid Id) : IRequest {
}

public record PhuLucHopDongDeleteCommandHandler : IRequestHandler<PhuLucHopDongDeleteCommand> {
    private readonly IRepository<PhuLucHopDong, Guid> PhuLucHopDong;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public PhuLucHopDongDeleteCommandHandler(IServiceProvider serviceProvider) {
        PhuLucHopDong = serviceProvider.GetRequiredService<IRepository<PhuLucHopDong, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = PhuLucHopDong.UnitOfWork;
    }

    public async Task Handle(PhuLucHopDongDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await PhuLucHopDong.GetOrderedSet()
           .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
