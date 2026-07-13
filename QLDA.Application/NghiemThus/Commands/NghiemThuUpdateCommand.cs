using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.NghiemThus.DTOs;
using QLDA.Application.Providers;
using QLDA.Domain.Entities;

namespace QLDA.Application.NghiemThus.Commands;

public record NghiemThuUpdateCommand(NghiemThuUpdateDto Dto, List<Guid>? PhuLucHopDongIds = null) : IRequest<NghiemThu>;

internal class NghiemThuUpdateCommandHandler : IRequestHandler<NghiemThuUpdateCommand, NghiemThu> {
    private readonly IRepository<NghiemThu, Guid> NghiemThu;
    private readonly IRepository<HopDong, Guid> HopDong;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<NghiemThuUpdateCommandHandler>();

    public NghiemThuUpdateCommandHandler(IServiceProvider serviceProvider) {
        NghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        HopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        _unitOfWork = NghiemThu.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<NghiemThu> Handle(NghiemThuUpdateCommand request, CancellationToken cancellationToken = default) {
        await ValidateAsync(request, cancellationToken);

        // Use AsNoTracking to avoid EF tracking conflicts with junction table
        var entity = await NghiemThu.GetQueryableSet()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Authorization check on existing entity's BuocId
        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        // Update scalar properties only (not navigation collections)
        entity.HopDongId = request.Dto.HopDongId;
        entity.SoBienBan = request.Dto.SoBienBan;
        entity.Dot = request.Dto.Dot;
        entity.Ngay = request.Dto.Ngay;
        entity.NoiDung = request.Dto.NoiDung;
        entity.GiaTri = request.Dto.GiaTri;

        // Sync junction table via raw delete/insert to avoid EF tracking issues
        await SyncNghiemThuPhuLucHopDongAsync(entity.Id, request.Dto.PhuLucHopDongIds, cancellationToken);

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

    private async Task SyncNghiemThuPhuLucHopDongAsync(Guid nghiemThuId, List<Guid>? newPhuLucHopDongIds, CancellationToken cancellationToken) {
        var dbContext = NghiemThu.UnitOfWork as DbContext;
        if (dbContext == null) return;

        // Delete existing junction records for this NghiemThu
        var existingRecords = await dbContext.Set<NghiemThuPhuLucHopDong>()
            .Where(x => x.LeftId == nghiemThuId)
            .ToListAsync(cancellationToken);

        if (existingRecords.Any()) {
            dbContext.Set<NghiemThuPhuLucHopDong>().RemoveRange(existingRecords);
        }

        // Insert new junction records (only if not already existing)
        if (newPhuLucHopDongIds?.Any() == true) {
            var existingRightIds = existingRecords.Select(x => x.RightId).ToHashSet();
            var newRecords = newPhuLucHopDongIds
                .Where(id => !existingRightIds.Contains(id))
                .Select(phuLucId => new NghiemThuPhuLucHopDong {
                    LeftId = nghiemThuId,
                    RightId = phuLucId
                })
                .ToList();

            if (newRecords.Any()) {
                await dbContext.Set<NghiemThuPhuLucHopDong>().AddRangeAsync(newRecords, cancellationToken);
            }
        }
    }

    #region  Private helper methods

    private async Task ValidateAsync(NghiemThuUpdateCommand request, CancellationToken cancellationToken) {

        ManagedException.ThrowIf(
            when: !await HopDong.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.HopDongId, cancellationToken),
            message: "Không tồn tại hợp đồng"
        );
        var query = NghiemThu.GetQueryableSet()
           .Where(e => e.HopDongId == request.Dto.HopDongId);

        ManagedException.ThrowIf(
             when: request.Dto.SoBienBan.IsNotNullOrWhitespace() && await query.AnyAsync(e => e.Id != request.Dto.Id && e.SoBienBan!.ToLower() == request.Dto.SoBienBan!.ToLower(), cancellationToken),
             message: "Số biên bản đã tồn tại"
         );

        ManagedException.ThrowIf(
            when: await query.AnyAsync(e => e.Id != request.Dto.Id && e.Dot!.ToLower() == request.Dto.Dot!.ToLower(), cancellationToken),
            message: "Đợt nghiệm thu đã tồn tại"
        );

    }
    private async Task UpdateAsync(NghiemThu entity, CancellationToken cancellationToken) {
        await NghiemThu.UpdateAsync(entity, cancellationToken);
    }

    #endregion
}