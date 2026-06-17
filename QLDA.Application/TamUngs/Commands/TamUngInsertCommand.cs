using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

namespace QLDA.Application.TamUngs.Commands;

public record TamUngInsertCommand(TamUngInsertDto Dto) : IRequest<TamUng>;

internal class TamUngInsertCommandHandler : IRequestHandler<TamUngInsertCommand, TamUng> {
    private readonly IRepository<TamUng, Guid> TamUng;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<TamUngInsertCommandHandler>();

    public TamUngInsertCommandHandler(IServiceProvider serviceProvider) {
        TamUng = serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = TamUng.UnitOfWork;
    }

    public async Task<TamUng> Handle(TamUngInsertCommand request, CancellationToken cancellationToken = default) {
        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

        await ValidateAsync(request, cancellationToken);

        var entity = request.Dto.ToEntity();

        if (_unitOfWork.HasTransaction) {
            await InsertAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await InsertAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        return entity;
    }

    #region  Private helper methods

    private async Task ValidateAsync(TamUngInsertCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(
            when: !await DuAn.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.DuAnId, cancellationToken),
            message: "Không tồn tại dự án");
        ManagedException.ThrowIf(
            when: await TamUng.GetQueryableSet().AnyAsync(e => e.HopDongId == request.Dto.HopDongId, cancellationToken: cancellationToken),
            message: "Hợp đồng chỉ được tạm ứng duy nhất 1 lần"
        );
        ManagedException.ThrowIf(
            when: await TamUng.GetQueryableSet().AnyAsync(e => e.SoPhieuChi!.ToLower() == request.Dto.SoPhieuChi!.ToLower(), cancellationToken),
            message: "Số phiếu chi đã tồn tại"
        );
        ManagedException.ThrowIf(
            when: await TamUng.GetQueryableSet().AnyAsync(e => e.SoBaoLanh!.ToLower() == request.Dto.SoBaoLanh!.ToLower(), cancellationToken),
            message: "Số bảo lãnh đã tồn tại"
        );

    }
    private async Task InsertAsync(TamUng entity, CancellationToken cancellationToken) {
        await TamUng.AddAsync(entity, cancellationToken);
    }

    #endregion
}