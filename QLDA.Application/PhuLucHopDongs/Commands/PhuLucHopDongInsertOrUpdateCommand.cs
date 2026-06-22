using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.Providers;

namespace QLDA.Application.PhuLucHopDongs.Commands;

public record PhuLucHopDongInsertOrUpdateCommand(PhuLucHopDong Entity) : IRequest {
}

internal class PhuLucHopDongInsertOrUpdateCommandHandler : IRequestHandler<PhuLucHopDongInsertOrUpdateCommand> {
    private readonly IRepository<PhuLucHopDong, Guid> PhuLucHopDong;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PhuLucHopDongInsertOrUpdateCommandHandler> _logger;

    public PhuLucHopDongInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<PhuLucHopDongInsertOrUpdateCommandHandler> logger) {
        PhuLucHopDong = serviceProvider.GetRequiredService<IRepository<PhuLucHopDong, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _logger = logger;
        _unitOfWork = PhuLucHopDong.UnitOfWork;
    }

    public async Task Handle(PhuLucHopDongInsertOrUpdateCommand request, CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = PhuLucHopDong.GetQueryableSet().Any(o => o.Id == request.Entity.Id);

                if (isExist) {
                    // For update: load entity to get BuocId for authorization check
                    var existingEntity = await PhuLucHopDong.GetQueryableSet()
                        .FirstOrDefaultAsync(o => o.Id == request.Entity.Id, cancellationToken);
                    ManagedException.ThrowIfNull(existingEntity);

                    // Check step authorization
                    await _auth.EnsureCanExecuteStepAsync(existingEntity.BuocId, _authContext, cancellationToken);

                    await PhuLucHopDong.UpdateAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                } else {
                    // For insert: check BuocId from request.Entity
                    // Check step authorization
                    await _auth.EnsureCanExecuteStepAsync(request.Entity.BuocId, _authContext, cancellationToken);

                    //Thêm dự án trước
                    await PhuLucHopDong.AddAsync(request.Entity, cancellationToken);
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