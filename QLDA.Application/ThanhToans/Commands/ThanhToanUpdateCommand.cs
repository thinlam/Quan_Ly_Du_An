using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Providers;
using QLDA.Application.ThanhToans.DTOs;

namespace QLDA.Application.ThanhToans.Commands;

public record ThanhToanUpdateCommand(ThanhToanUpdateDto Dto) : IRequest<ThanhToan>
{
    public List<Guid> NghiemThuIds { get; set; } = [];
}

internal class ThanhToanUpdateCommandHandler : IRequestHandler<ThanhToanUpdateCommand, ThanhToan>
{
    private readonly IRepository<ThanhToan, Guid> ThanhToan;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ThanhToanUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        ThanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = ThanhToan.UnitOfWork;
    }

    public async Task<ThanhToan> Handle(ThanhToanUpdateCommand request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(request, cancellationToken);

        var entity = await ThanhToan.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Chỉ Phòng Kế Hoạch - Tài chính mới có quyền thực hiện thao tác này
        ManagedException.ThrowIf(!_authContext.HasKhtcBypass,
            "Chỉ Phòng Kế Hoạch - Tài chính có quyền thực hiện thao tác này");

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);
        entity.Update(request.Dto);

        if (_unitOfWork.HasTransaction)
        {
            await UpdateAsync(entity, cancellationToken);
        }
        else
        {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        return entity;
    }
    #region  Private helper methods

    private async Task ValidateAsync(ThanhToanUpdateCommand request, CancellationToken cancellationToken)
    {
    }
    private async Task UpdateAsync(ThanhToan entity, CancellationToken cancellationToken)
    {
        await ThanhToan.UpdateAsync(entity, cancellationToken);
    }

    #endregion
}