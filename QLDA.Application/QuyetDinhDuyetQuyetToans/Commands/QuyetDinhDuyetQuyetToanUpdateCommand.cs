using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.QuyetDinhDuyetQuyetToans.DTOs;

namespace QLDA.Application.QuyetDinhDuyetQuyetToans.Commands;

public record QuyetDinhDuyetQuyetToanUpdateCommand(QuyetDinhDuyetQuyetToanUpdateDto Dto) : IRequest<QuyetDinhDuyetQuyetToan>;

internal class QuyetDinhDuyetQuyetToanUpdateCommandHandler : IRequestHandler<QuyetDinhDuyetQuyetToanUpdateCommand, QuyetDinhDuyetQuyetToan> {
    private readonly IRepository<QuyetDinhDuyetQuyetToan, Guid> QuyetDinhDuyetQuyetToan;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuyetDinhDuyetQuyetToanUpdateCommandHandler> _logger;

    public QuyetDinhDuyetQuyetToanUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetQuyetToanUpdateCommandHandler> logger) {
        QuyetDinhDuyetQuyetToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetQuyetToan, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        _logger = logger;
        _unitOfWork = QuyetDinhDuyetQuyetToan.UnitOfWork;
    }

    public async Task<QuyetDinhDuyetQuyetToan> Handle(QuyetDinhDuyetQuyetToanUpdateCommand request, CancellationToken cancellationToken = default) {
        try {
            var entity = QuyetDinhDuyetQuyetToan.GetQueryableSet().Where(x => x.Id == request.Dto.Id).FirstOrDefault();
            ManagedException.ThrowIfNull(entity, "Không tồn tại dữ liệu cần cập nhật");
            request.Dto.ToEntity(entity);

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await QuyetDinhDuyetQuyetToan.UpdateAsync(entity, cancellationToken);
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
