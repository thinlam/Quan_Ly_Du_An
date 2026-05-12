using System.Data;
using BuildingBlocks.Domain.Providers;
using MediatR;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.BanGiaoHoSos.Commands;

public record BanGiaoHoSoInsertCommand(BanGiaoHoSoInsertDto Dto) : IRequest<BanGiaoHoSo>;

internal class BanGiaoHoSoInsertCommandHandler : IRequestHandler<BanGiaoHoSoInsertCommand, BanGiaoHoSo> {
    private readonly IRepository<BanGiaoHoSo, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProvider _userProvider;

    public BanGiaoHoSoInsertCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    }

    public async Task<BanGiaoHoSo> Handle(BanGiaoHoSoInsertCommand request, CancellationToken cancellationToken = default) {
        var entity = request.Dto.ToEntity();
        entity.UserId = _userProvider.Id;  // Lấy từ JWT token qua IUserProvider

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
