using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuAns.DTOs;

namespace QLDA.Application.DuAns.Commands;

public record DuAnInsertCommand(DuAnInsertDto Dto) : IRequest<DuAn>;

internal class DuAnInsertCommandHandler : IRequestHandler<DuAnInsertCommand, DuAn> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucNguonVon, int> DanhMucNguonVon;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<DuAnInsertCommandHandler>();

    public DuAnInsertCommandHandler(IServiceProvider serviceProvider) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucNguonVon = serviceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();
        _unitOfWork = DuAn.UnitOfWork;
    }

    public async Task<DuAn> Handle(DuAnInsertCommand request, CancellationToken cancellationToken = default) {

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

    private async Task ValidateAsync(DuAnInsertCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(request.Dto.DanhSachNguonVon?.Count > 0 && await DanhMucNguonVon.GetQueryableSet().CountAsync(e => request.Dto.DanhSachNguonVon!.Contains(e.Id), cancellationToken) != request.Dto.DanhSachNguonVon!.Distinct().Count(), "Nguồn vốn không hợp lệ");

    }

    private async Task InsertAsync(DuAn entity, CancellationToken cancellationToken) {

        await DuAn.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Initialize the materialized path for the new entity
        DuAn? parent = null;
        if (entity.ParentId.HasValue) {
            parent = await DuAn.GetQueryableSet()
                .FirstOrDefaultAsync(e => e.Id == entity.ParentId.Value, cancellationToken);
        }

        DuAn.InitializeNode(entity, parent);

        // Auto-set DuToanBanDauId from first DuToan if not already set
        if (!entity.DuToanBanDauId.HasValue && entity.DuToans?.Count > 0) {
            var firstDuToan = entity.DuToans.OrderBy(d => d.Index).FirstOrDefault();
            if (firstDuToan != null) {
                entity.DuToanBanDauId = firstDuToan.SoDuToan;
                entity.SoDuToanBanDau = firstDuToan.SoDuToan;
                await DuAn.UpdateAsync(entity, cancellationToken);
            }
        }
    }

    #endregion
}