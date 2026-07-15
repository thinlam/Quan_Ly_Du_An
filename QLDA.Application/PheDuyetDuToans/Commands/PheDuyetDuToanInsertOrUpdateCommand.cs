using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;

namespace QLDA.Application.PheDuyetDuToans.Commands;

public record PheDuyetDuToanInsertOrUpdateCommand(PheDuyetDuToan Entity) : IRequest {
}

internal class PheDuyetDuToanInsertOrUpdateCommandHandler : IRequestHandler<PheDuyetDuToanInsertOrUpdateCommand> {
    private readonly IRepository<PheDuyetDuToan, Guid> PheDuyetDuToan;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<DanhMucChucVu, int> DanhMucChucVu;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PheDuyetDuToanInsertOrUpdateCommandHandler> _logger;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public PheDuyetDuToanInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<PheDuyetDuToanInsertOrUpdateCommandHandler> logger) {
        PheDuyetDuToan = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        DanhMucChucVu = serviceProvider.GetRequiredService<IRepository<DanhMucChucVu, int>>();
        _logger = logger;
        _unitOfWork = PheDuyetDuToan.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task Handle(PheDuyetDuToanInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default) {
        try {
            await _authManager.EnsureCanExecuteAsync(request.Entity.BuocId, request.Entity.DuAnId, _authContext, cancellationToken);
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");
            ManagedException.ThrowIf(
                request.Entity.ChucVuId > 0 &&
                !DanhMucChucVu.GetQueryableSet().Any(e => e.Id == request.Entity.ChucVuId),
                "Không tồn tại chức vụ này");
            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = PheDuyetDuToan.GetQueryableSet().Any(o => o.Id == request.Entity.Id);
                if (isExist) {
                    await PheDuyetDuToan.UpdateAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                } else {
                    //Thêm dự án trước
                    await PheDuyetDuToan.AddAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}