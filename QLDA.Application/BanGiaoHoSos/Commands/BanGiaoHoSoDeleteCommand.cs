using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Commands;

public record BanGiaoHoSoDeleteCommand(Guid Id) : IRequest;

internal class BanGiaoHoSoDeleteCommandHandler : IRequestHandler<BanGiaoHoSoDeleteCommand> {
    private readonly IRepository<BanGiaoHoSo, Guid> _repository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public BanGiaoHoSoDeleteCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task Handle(BanGiaoHoSoDeleteCommand request, CancellationToken cancellationToken = default) {
        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Chỉ cho phép xóa khi TrangThai = 1 (Khởi tạo)
        if (entity.TrangThai != ETrangThaiBanGiao.KhoiTao) {
            throw new InvalidOperationException("Chỉ có thể xóa bản giao hồ sơ ở trạng thái 'Khởi tạo'");
        }

        entity.IsDeleted = true;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}
