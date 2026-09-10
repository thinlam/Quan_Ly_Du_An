
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DuAns.Commands;

public record DuAnUpdatePhaseCommand(Guid DuAnId, DuAnBuoc? DuAnBuoc) : IRequest;


internal class DuAnUpdatePhaseCommandHandler : IRequestHandler<DuAnUpdatePhaseCommand> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucGiaiDoan, int> DanhMucGiaiDoan;
    private readonly IUnitOfWork _unitOfWork;

    public DuAnUpdatePhaseCommandHandler(IServiceProvider serviceProvider) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucGiaiDoan = serviceProvider.GetRequiredService<IRepository<DanhMucGiaiDoan, int>>();
        _unitOfWork = DanhMucGiaiDoan.UnitOfWork;
    }

    public async Task Handle(DuAnUpdatePhaseCommand request, CancellationToken cancellationToken) {
        if (request.DuAnBuoc is null)
            return;

        // Validate tồn tại dự án và bước
        if (_unitOfWork.HasTransaction) {
            await UpdateCurrentPhaseAsync(request, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await UpdateCurrentPhaseAsync(request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }
    #region Private helper methods
    private async Task UpdateCurrentPhaseAsync(DuAnUpdatePhaseCommand request, CancellationToken cancellationToken) {
        // BuocHienTaiId đã được DuAnUpdateStepCommand guard chỉ tiến-tới, nên giai đoạn
        // hợp lệ luôn là phase của bước hiện tại. Không so Stt của DmGiaiDoan: các phase
        // mới (id 15-22) đều có Stt trùng (0) khiến guard cũ không bao giờ nâng phase.
        var giaiDoanId = await DuAn.GetQueryableSet()
            .Where(e => e.Id == request.DuAnId)
            .Select(e => e.BuocHienTai != null && e.BuocHienTai.Buoc != null
                ? e.BuocHienTai.Buoc.GiaiDoanId
                : null)
            .FirstOrDefaultAsync(cancellationToken);

        if (giaiDoanId == null)
            return;

        await SetPhase(request.DuAnId, giaiDoanId.Value, cancellationToken);
    }
    private async Task SetPhase(Guid duAnId, int giaiDoanId, CancellationToken cancellationToken = default) {
        await DuAn.GetQueryableSet()
            .Where(e => e.Id == duAnId)
            .ExecuteUpdateAsync(setCall => setCall.SetProperty(e => e.GiaiDoanHienTaiId, giaiDoanId),
                cancellationToken);
    }

    #endregion
}