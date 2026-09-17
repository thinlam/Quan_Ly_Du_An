using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.ThanhToans.DTOs;

namespace QLDA.Application.ThanhToans.Commands;

public record ThanhToanInsertCommand(ThanhToanInsertDto Dto) : IRequest<ThanhToan>;

internal class ThanhToanInsertCommandHandler : IRequestHandler<ThanhToanInsertCommand, ThanhToan>
{
    private readonly IRepository<ThanhToan, Guid> ThanhToan;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<NghiemThu, Guid> NghiemThu;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ThanhToanInsertCommandHandler(IServiceProvider serviceProvider)
    {
        ThanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        NghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = ThanhToan.UnitOfWork;
    }

    public async Task<ThanhToan> Handle(ThanhToanInsertCommand request, CancellationToken cancellationToken = default)
    {
        // Chỉ Phòng Kế Hoạch - Tài chính mới có quyền thực hiện thao tác này
        ManagedException.ThrowIf(!_authContext.HasKhtcBypass,
            "Chỉ Phòng Kế Hoạch - Tài chính có quyền thực hiện thao tác này");

        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

        await ValidateAsync(request, cancellationToken);

        var entity = request.Dto.ToEntity();

        if (_unitOfWork.HasTransaction)
        {
            await InsertAsync(entity, cancellationToken);
        }
        else
        {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await InsertAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }


        return entity!;

    }

    #region  Private helper methods

    private async Task ValidateAsync(ThanhToanInsertCommand request, CancellationToken cancellationToken)
    {
        ManagedException.ThrowIf(!await DuAn.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.DuAnId, cancellationToken: cancellationToken),
           "Không tồn tại dự án");
        ManagedException.ThrowIf(!await NghiemThu.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.NghiemThuId, cancellationToken: cancellationToken),
           "Không tồn tại đợt nghiệm thu");
    }

    private async Task InsertAsync(ThanhToan entity, CancellationToken cancellationToken)
    {
        await ThanhToan.AddAsync(entity, cancellationToken);
    }

    #endregion
}