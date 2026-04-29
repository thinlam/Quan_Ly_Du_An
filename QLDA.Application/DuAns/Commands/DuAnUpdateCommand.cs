using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.DuAns.DTOs;
using QLDA.Application.DuToans.DTOs;
using QLDA.Application.KeHoachVons.DTOs;

namespace QLDA.Application.DuAns.Commands;

public record DuAnUpdateCommand(DuAnUpdateModel Model) : IRequest<DuAn>;

internal class DuAnUpdateCommandHandler : IRequestHandler<DuAnUpdateCommand, DuAn> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DuToan, Guid> DuToan;
    private readonly IRepository<KeHoachVon, Guid> KeHoachVon;
    private readonly IRepository<DanhMucNguonVon, int> DanhMucNguonVon;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<DuAnUpdateCommandHandler>();

    public DuAnUpdateCommandHandler(IServiceProvider serviceProvider) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DuToan = serviceProvider.GetRequiredService<IRepository<DuToan, Guid>>();
        KeHoachVon = serviceProvider.GetRequiredService<IRepository<KeHoachVon, Guid>>();
        DanhMucNguonVon = serviceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();
        _unitOfWork = DuAn.UnitOfWork;
    }

    public async Task<DuAn> Handle(DuAnUpdateCommand request, CancellationToken cancellationToken = default) {
        await ValidateAsync(request, cancellationToken);

        var entity = await DuAn.GetQueryableSet()
            .Include(e => e.DuAnNguonVons)
            .Include(e => e.DuAnChiuTrachNhiemXuLys)
            .Include(e => e.DuToans)
            .Include(e => e.KeHoachVons)
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
        }, cancellationToken);

        await SyncHelper.SyncCollection(KeHoachVon, entity.KeHoachVons, [.. request.Model.KeHoachVons?.Select(e => e.ToEntity(entity.Id)) ?? []], (existing, request) => {
            existing.NguonVonId = request.NguonVonId;
            existing.Nam = request.Nam;
            existing.SoVon = request.SoVon;
            existing.SoVonDieuChinh = request.SoVonDieuChinh;
            existing.SoQuyetDinh = request.SoQuyetDinh;
            existing.NgayKy = request.NgayKy;
            existing.GhiChu = request.GhiChu;
        }
          , cancellationToken);

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

    private async Task ValidateAsync(DuAnUpdateCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(request.Model.DanhSachNguonVon?.Count > 0 && await DanhMucNguonVon.GetQueryableSet().CountAsync(e => request.Model.DanhSachNguonVon!.Contains(e.Id), cancellationToken) != request.Model.DanhSachNguonVon!.Distinct().Count(), "Nguồn vốn không hợp lệ");
        // Add more validation if needed
    }

    private async Task UpdateAsync(DuAn entity, CancellationToken cancellationToken) {
        await DuAn.UpdateAsync(entity, cancellationToken);
    }
    #endregion
}