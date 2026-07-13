using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;

namespace QLDA.Application.QuyetDinhDuyetQuyetToans.Commands;

public record QuyetDinhDuyetQuyetToanInsertOrUpdateCommand(QuyetDinhDuyetQuyetToan Entity) : IRequest {
}

internal class
    QuyetDinhDuyetQuyetToanInsertOrUpdateCommandHandler : IRequestHandler<QuyetDinhDuyetQuyetToanInsertOrUpdateCommand> {
    private readonly IRepository<QuyetDinhDuyetQuyetToan, Guid> QuyetDinhDuyetQuyetToan;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuyetDinhDuyetQuyetToanInsertOrUpdateCommandHandler> _logger;

    public QuyetDinhDuyetQuyetToanInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetQuyetToanInsertOrUpdateCommandHandler> logger) {
        QuyetDinhDuyetQuyetToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetQuyetToan, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _logger = logger;
        _unitOfWork = QuyetDinhDuyetQuyetToan.UnitOfWork;
    }

    public async Task Handle(QuyetDinhDuyetQuyetToanInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default) {
        await _authManager.EnsureCanExecuteAsync(request.Entity.BuocId, request.Entity.DuAnId, _authContext, cancellationToken);
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = QuyetDinhDuyetQuyetToan.GetQueryableSet().Any(o => o.Id == request.Entity.Id);
                if (isExist) {
                    await QuyetDinhDuyetQuyetToan.UpdateAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                } else {
                    //Thêm dự án trước
                    await QuyetDinhDuyetQuyetToan.AddAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                //Cập nhật quy trình
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}