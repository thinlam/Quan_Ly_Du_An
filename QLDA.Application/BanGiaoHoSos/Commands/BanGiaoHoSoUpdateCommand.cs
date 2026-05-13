using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Commands;

public record BanGiaoHoSoUpdateCommand(BanGiaoHoSoUpdateModel Model) : IRequest<BanGiaoHoSo>;

internal class BanGiaoHoSoUpdateCommandHandler : IRequestHandler<BanGiaoHoSoUpdateCommand, BanGiaoHoSo> {
    private readonly IRepository<BanGiaoHoSo, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public BanGiaoHoSoUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<BanGiaoHoSo> Handle(BanGiaoHoSoUpdateCommand request, CancellationToken cancellationToken = default) {
        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Chỉ cho phép cập nhật khi TrangThai = 1 (Khởi tạo)
        if (entity.TrangThai != ETrangThaiBanGiao.KhoiTao) {
            throw new InvalidOperationException("Chỉ có thể cập nhật bản giao hồ sơ ở trạng thái 'Khởi tạo'");
        }

        entity.Update(request.Model);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
