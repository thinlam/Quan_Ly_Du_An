using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

namespace QLDA.Application.TamUngs.Commands;

public record TamUngUpdateCommand(TamUngUpdateDto Dto) : IRequest<TamUng>;

internal class TamUngUpdateCommandHandler : IRequestHandler<TamUngUpdateCommand, TamUng> {
    private readonly IRepository<TamUng, Guid> TamUng;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<TamUngUpdateCommandHandler>();

    public TamUngUpdateCommandHandler(IServiceProvider serviceProvider) {
        TamUng = serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = TamUng.UnitOfWork;
    }

    public async Task<TamUng> Handle(TamUngUpdateCommand request, CancellationToken cancellationToken = default) {
        await ValidateAsync(request, cancellationToken);

        var entity = await TamUng.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        entity.Update(request.Dto);

        if (_unitOfWork.HasTransaction) {
            await UpdateAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        return entity;
    }
    #region  Private helper methods

    private async Task ValidateAsync(TamUngUpdateCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(
            when: await TamUng.GetQueryableSet().AnyAsync(e => e.HopDongId == request.Dto.HopDongId && e.Id != request.Dto.Id, cancellationToken: cancellationToken),
            message: "Hợp đồng chỉ được tạm ứng duy nhất 1 lần"
        );
        ManagedException.ThrowIf(
            when: await TamUng.GetQueryableSet().AnyAsync(e => e.SoPhieuChi!.ToLower() == request.Dto.SoPhieuChi!.ToLower() && e.Id != request.Dto.Id, cancellationToken),
            message: "Số phiếu chi đã tồn tại"
        );
        ManagedException.ThrowIf(
            when: await TamUng.GetQueryableSet().AnyAsync(e => e.SoBaoLanh!.ToLower() == request.Dto.SoBaoLanh!.ToLower() && e.Id != request.Dto.Id, cancellationToken),
            message: "Số bảo lãnh đã tồn tại"
        );

    }
    private async Task UpdateAsync(TamUng entity, CancellationToken cancellationToken) {
        await TamUng.UpdateAsync(entity, cancellationToken);
    }

    #endregion
}