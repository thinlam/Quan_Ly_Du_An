using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.PheDuyetDuToans.Commands;

public record PheDuyetDuToanDeleteCommand(Guid Id) : IRequest<int>
{
}

public record PheDuyetDuToanDeleteCommandHandler : IRequestHandler<PheDuyetDuToanDeleteCommand, int>
{
    private readonly IRepository<PheDuyetDuToan, Guid> PheDuyetDuToan;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public PheDuyetDuToanDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        PheDuyetDuToan =serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _unitOfWork = PheDuyetDuToan.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<int> Handle(PheDuyetDuToanDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await PheDuyetDuToan.GetOrderedSet()
            // .Include(o => o.DanhSachToTrinh)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}