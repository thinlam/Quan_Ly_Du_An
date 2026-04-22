using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.DuAns.DTOs;
using QLDA.Application.DuToans.DTOs;

namespace QLDA.Application.DuAns.Commands;

public record DuAnUpdateCommand(DuAnUpdateModel Model) : IRequest<DuAn>;

internal class DuAnUpdateCommandHandler : IRequestHandler<DuAnUpdateCommand, DuAn> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DuToan, Guid> DuToan;
    private readonly IRepository<DanhMucNguonVon, int> DanhMucNguonVon;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<DuAnUpdateCommandHandler>();

    public DuAnUpdateCommandHandler(IServiceProvider serviceProvider) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DuToan = serviceProvider.GetRequiredService<IRepository<DuToan, Guid>>();
        DanhMucNguonVon = serviceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();
        _unitOfWork = DuAn.UnitOfWork;
    }

    public async Task<DuAn> Handle(DuAnUpdateCommand request, CancellationToken cancellationToken = default) {
        await ValidateAsync(request, cancellationToken);

        var entity = await DuAn.GetQueryableSet()
            .Include(e => e.DuAnNguonVons)
            .Include(e => e.DuAnChiuTrachNhiemXuLys)
            .Include(e => e.DuToans)
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Store the original ParentId to check if it changed
        var originalParentId = entity.ParentId;
        
        entity.Update(request.Model);

        // Check if ParentId has changed and handle materialized path update
        if (originalParentId != entity.ParentId) {
            DuAn? newParent = null;
            if (entity.ParentId.HasValue) {
                newParent = await DuAn.GetQueryableSet()
                    .FirstOrDefaultAsync(e => e.Id == entity.ParentId.Value, cancellationToken);
            }
            
            await DuAn.MoveNodeAsync(entity, newParent, cancellationToken);
        }

        await SyncHelper.SyncCollection(DuToan, entity.DuToans, [.. request.Model.DuToans?.Select(e => e.ToEntity(entity.Id)) ?? []], (existing, request) => {
            existing.SoDuToan = request.SoDuToan;
            existing.NamDuToan = request.NamDuToan;
            existing.SoQuyetDinhDuToan = request.SoQuyetDinhDuToan;
            existing.NgayKyDuToan = request.NgayKyDuToan;
            existing.GhiChu = request.GhiChu;
        }
          , cancellationToken);

        // Update SoDuToanBanDau and SoDuToanCuoiCung based on the updated DuToans list
        if (entity.DuToans != null && entity.DuToans.Count > 0) {
            var sortedDuToans = entity.DuToans.Where(d => !d.IsDeleted).OrderBy(d => d.Index).ToList();
            
            // Set initial budget from first DuToan
            if (sortedDuToans.Count > 0) {
                var firstDuToan = sortedDuToans.First();
                entity.SoDuToanBanDau = firstDuToan.SoDuToan;
            }
            
            // Set adjusted/final budget from last DuToan if count > 1
            if (sortedDuToans.Count > 1) {
                var lastDuToan = sortedDuToans.Last();
                entity.SoDuToanCuoiCung = lastDuToan.SoDuToan;
            } else {
                // If only 1 DuToan, set SoDuToanCuoiCung to null
                entity.SoDuToanCuoiCung = null;
            }
        }

        if (_unitOfWork.HasTransaction) {
            await UpdateAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }

        // Query latest DuToan after save to include newly added records from SyncCollection
        var duToanMoiNhat = await DuToan.GetQueryableSet()
            .Where(d => d.DuAnId == entity.Id && !d.IsDeleted)
            .OrderByDescending(d => d.NamDuToan)
            .ThenByDescending(d => d.NgayKyDuToan)
            .FirstOrDefaultAsync(cancellationToken);

        if (duToanMoiNhat != null) {
            entity.DuToanHienTaiId = duToanMoiNhat.Id;
            entity.SoDuToan = duToanMoiNhat.SoDuToan;
            entity.NamDuToan = duToanMoiNhat.NamDuToan;
            entity.SoQuyetDinhDuToan = duToanMoiNhat.SoQuyetDinhDuToan;
            entity.NgayKyDuToan = duToanMoiNhat.NgayKyDuToan;
            await DuAn.UpdateAsync(entity, cancellationToken);
        } else if (entity.DuToanHienTaiId != null) {
            entity.DuToanHienTaiId = null;
            entity.SoDuToan = 0;
            entity.NamDuToan = 0;
            entity.SoQuyetDinhDuToan = null;
            entity.NgayKyDuToan = null;
            await DuAn.UpdateAsync(entity, cancellationToken);
        }

        return entity;
    }

    #region  Private helper methods

    private async Task ValidateAsync(DuAnUpdateCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(request.Model.DanhSachNguonVon?.Count > 0 && await DanhMucNguonVon.GetQueryableSet().CountAsync(e => request.Model.DanhSachNguonVon!.Contains(e.Id), cancellationToken) != request.Model.DanhSachNguonVon!.Distinct().Count(), "Nguồn vốn không hợp lệ");
        // Add more validation if needed
    }

    private async Task UpdateAsync(DuAn entity, CancellationToken cancellationToken) {
        await DuAn.UpdateAsync(entity, cancellationToken);
    }
    #endregion
}