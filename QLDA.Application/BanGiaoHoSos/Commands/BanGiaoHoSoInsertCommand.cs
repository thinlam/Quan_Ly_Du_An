using System.Data;
using MediatR;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.BanGiaoHoSos.Commands;

public record BanGiaoHoSoInsertCommand(BanGiaoHoSoInsertDto Dto) : IRequest<BanGiaoHoSo>;

internal class BanGiaoHoSoInsertCommandHandler : IRequestHandler<BanGiaoHoSoInsertCommand, BanGiaoHoSo> {
    private readonly IRepository<BanGiaoHoSo, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public BanGiaoHoSoInsertCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<BanGiaoHoSo> Handle(BanGiaoHoSoInsertCommand request, CancellationToken cancellationToken = default) {
        var entity = request.Dto.ToEntity();
        // CreatedBy được tự động set bởi EF interceptor từ JWT token – không cần gán thủ công

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
