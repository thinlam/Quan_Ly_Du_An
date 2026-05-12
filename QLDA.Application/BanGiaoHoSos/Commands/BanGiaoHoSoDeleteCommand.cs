using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Commands;

public record BanGiaoHoSoDeleteCommand(Guid Id) : IRequest;

internal class BanGiaoHoSoDeleteCommandHandler : IRequestHandler<BanGiaoHoSoDeleteCommand> {
    private readonly IRepository<BanGiaoHoSo, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public BanGiaoHoSoDeleteCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task Handle(BanGiaoHoSoDeleteCommand request, CancellationToken cancellationToken = default) {
        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Chỉ cho phép xóa khi TrangThai = 0 (Khởi tạo)
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
