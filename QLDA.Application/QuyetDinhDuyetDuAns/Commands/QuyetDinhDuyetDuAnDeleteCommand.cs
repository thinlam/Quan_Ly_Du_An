using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.QuyetDinhDuyetDuAns.Commands;

public record QuyetDinhDuyetDuAnDeleteCommand(Guid Id) : IRequest<int>
{
}

public record QuyetDinhDuyetDuAnDeleteCommandHandler : IRequestHandler<QuyetDinhDuyetDuAnDeleteCommand, int>
{
    private readonly IRepository<QuyetDinhDuyetDuAn, Guid> QuyetDinhDuyetDuAn;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public QuyetDinhDuyetDuAnDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        QuyetDinhDuyetDuAn =serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuAn, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _unitOfWork = QuyetDinhDuyetDuAn.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<int> Handle(QuyetDinhDuyetDuAnDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await QuyetDinhDuyetDuAn.GetOrderedSet()
            // .Include(o => o.DanhSachToTrinh)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}