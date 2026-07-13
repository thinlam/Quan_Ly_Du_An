using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;

namespace QLDA.Application.QuyetDinhLapBenMoiThaus.Commands;

public record QuyetDinhLapBenMoiThauInsertOrUpdateCommand(QuyetDinhLapBenMoiThau Entity) : IRequest {
}

internal class
    QuyetDinhLapBenMoiThauInsertOrUpdateCommandHandler : IRequestHandler<QuyetDinhLapBenMoiThauInsertOrUpdateCommand> {
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IRepository<QuyetDinhLapBenMoiThau, Guid> QuyetDinhLapBenMoiThau;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork UnitOfWork;
    private readonly ILogger<QuyetDinhLapBenMoiThauInsertOrUpdateCommandHandler> Logger;

    public QuyetDinhLapBenMoiThauInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhLapBenMoiThauInsertOrUpdateCommandHandler> logger) {
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        QuyetDinhLapBenMoiThau = serviceProvider.GetRequiredService<IRepository<QuyetDinhLapBenMoiThau, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        Logger = logger;
        UnitOfWork = QuyetDinhLapBenMoiThau.UnitOfWork;
    }

    public async Task Handle(QuyetDinhLapBenMoiThauInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default) {
        try {
            await _authManager.EnsureCanExecuteAsync(request.Entity.BuocId, request.Entity.DuAnId, _authContext, cancellationToken);

            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");

            using (await UnitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = QuyetDinhLapBenMoiThau.GetQueryableSet().Any(o => o.Id == request.Entity.Id);
                if (isExist) {
                } else {
                    await QuyetDinhLapBenMoiThau.AddAsync(request.Entity, cancellationToken);
                    await UnitOfWork.SaveChangesAsync(cancellationToken);
                }

                await UnitOfWork.SaveChangesAsync(cancellationToken);
                await UnitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            Logger.LogError(ex, ex.Message);
            throw;
        }
    }
}