using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.QuyetDinhDuyetQuyetToans.DTOs;

namespace QLDA.Application.QuyetDinhDuyetQuyetToans.Commands;

public record QuyetDinhDuyetQuyetToanInsertCommand(QuyetDinhDuyetQuyetToanInsertDto Dto) : IRequest<QuyetDinhDuyetQuyetToan>;

internal class QuyetDinhDuyetQuyetToanInsertCommandHandler : IRequestHandler<QuyetDinhDuyetQuyetToanInsertCommand, QuyetDinhDuyetQuyetToan> {
    private readonly IRepository<QuyetDinhDuyetQuyetToan, Guid> QuyetDinhDuyetQuyetToan;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuyetDinhDuyetQuyetToanInsertCommandHandler> _logger;

    public QuyetDinhDuyetQuyetToanInsertCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetQuyetToanInsertCommandHandler> logger) {
        QuyetDinhDuyetQuyetToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetQuyetToan, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _logger = logger;
        _unitOfWork = QuyetDinhDuyetQuyetToan.UnitOfWork;
    }

    public async Task<QuyetDinhDuyetQuyetToan> Handle(QuyetDinhDuyetQuyetToanInsertCommand request, CancellationToken cancellationToken = default) {
        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Dto.DuAnId),
                "Không tồn tại dự án");

            var entity = request.Dto.ToEntity();

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await QuyetDinhDuyetQuyetToan.AddAsync(entity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }

            return entity;
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}