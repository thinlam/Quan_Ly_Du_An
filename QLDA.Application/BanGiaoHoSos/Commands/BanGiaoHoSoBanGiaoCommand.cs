using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Commands;

/// <summary>
/// Thực hiện bàn giao hồ sơ: đổi trạng thái 1→2, set NgayBanGiao, lưu biên bản
/// </summary>
public record BanGiaoHoSoBanGiaoCommand(Guid Id, DateOnly? NgayBanGiao, long? PhongBanNhanId) : IRequest<BanGiaoHoSo>;

internal class BanGiaoHoSoBanGiaoCommandHandler : IRequestHandler<BanGiaoHoSoBanGiaoCommand, BanGiaoHoSo> {
    private readonly IRepository<BanGiaoHoSo, Guid> _repository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public BanGiaoHoSoBanGiaoCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<BanGiaoHoSo> Handle(BanGiaoHoSoBanGiaoCommand request, CancellationToken cancellationToken = default) {
        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        entity.TrangThai = ETrangThaiBanGiao.DaBanGiao;
        entity.NgayBanGiao = request.NgayBanGiao?.ToStartOfDayUtc() ?? DateTimeOffset.UtcNow;
        entity.PhongBanNhanId = request.PhongBanNhanId;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
