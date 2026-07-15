using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.KeHoachLuaChonNhaThaus.DTOs;

namespace QLDA.Application.KeHoachLuaChonNhaThaus.Commands;

public record KeHoachLuaChonNhaThauUpdateCommand(KeHoachLuaChonNhaThauUpdateDto Dto) : IRequest<KeHoachLuaChonNhaThau>;

internal class KeHoachLuaChonNhaThauUpdateCommandHandler : IRequestHandler<KeHoachLuaChonNhaThauUpdateCommand, KeHoachLuaChonNhaThau> {
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public KeHoachLuaChonNhaThauUpdateCommandHandler(IServiceProvider serviceProvider) {
        KeHoachLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
        _unitOfWork = KeHoachLuaChonNhaThau.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<KeHoachLuaChonNhaThau> Handle(KeHoachLuaChonNhaThauUpdateCommand request, CancellationToken cancellationToken = default) {
        var entity = await KeHoachLuaChonNhaThau.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.Update(request.Dto);

        if (_unitOfWork.HasTransaction) {
            await UpdateAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        return entity!;
    }
    #region  Private helper methods

    private async Task UpdateAsync(KeHoachLuaChonNhaThau entity, CancellationToken cancellationToken) {
        await KeHoachLuaChonNhaThau.UpdateAsync(entity, cancellationToken);
    }

    #endregion
}