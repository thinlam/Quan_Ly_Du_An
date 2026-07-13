using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.QuyetDinhDuyetQuyetToans.Commands;

public record QuyetDinhDuyetQuyetToanDeleteCommand(Guid Id) : IRequest<int> {
}

public record QuyetDinhDuyetQuyetToanDeleteCommandHandler : IRequestHandler<QuyetDinhDuyetQuyetToanDeleteCommand, int> {
    private readonly IRepository<QuyetDinhDuyetQuyetToan, Guid> QuyetDinhDuyetQuyetToan;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDuyetQuyetToanDeleteCommandHandler(IServiceProvider serviceProvider) {
        QuyetDinhDuyetQuyetToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetQuyetToan, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = QuyetDinhDuyetQuyetToan.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDuyetQuyetToanDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await QuyetDinhDuyetQuyetToan.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}