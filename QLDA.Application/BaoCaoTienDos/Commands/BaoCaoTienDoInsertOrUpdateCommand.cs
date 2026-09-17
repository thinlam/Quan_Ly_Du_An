using System.Data;
using BuildingBlocks.Domain.Entities.Abstractions;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;

namespace QLDA.Application.BaoCaoTienDos.Commands;

public record BaoCaoTienDoInsertOrUpdateCommand(BaoCaoTienDo Entity) : IRequest {
}

internal class BaoCaoTienDoInsertOrUpdateCommandHandler : IRequestHandler<BaoCaoTienDoInsertOrUpdateCommand> {
    private readonly IRepository<BaoCaoTienDo, Guid> BaoCaoTienDo;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly ILogger<BaoCaoTienDoInsertOrUpdateCommandHandler> _logger;

    public BaoCaoTienDoInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<BaoCaoTienDoInsertOrUpdateCommandHandler> logger) {
        BaoCaoTienDo = serviceProvider.GetRequiredService<IRepository<BaoCaoTienDo, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _logger = logger;
        _unitOfWork = BaoCaoTienDo.UnitOfWork;
    }

    public async Task Handle(BaoCaoTienDoInsertOrUpdateCommand request, CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");

          
            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = BaoCaoTienDo.GetQueryableSet().Any(o => o.Id == request.Entity.Id);
                var entity = request.Entity;
                await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);
                await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);
                if (isExist) {
                    await BaoCaoTienDo.UpdateAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                } else {
                    //Thêm dự án trước
                    await BaoCaoTienDo.AddAsync(request.Entity, cancellationToken);
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
