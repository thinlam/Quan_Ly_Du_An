using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucBuocs.DTOs;

namespace QLDA.Application.DanhMucBuocs.Commands;

public record DanhMucBuocUpdateCommand(DanhMucBuocUpdateDto Dto) : IRequest<DanhMucBuoc>;

internal class DanhMucBuocUpdateCommandHandler : IRequestHandler<DanhMucBuocUpdateCommand, DanhMucBuoc> {
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<DanhMucManHinh, int> DanhMucManHinh;
    private readonly IUnitOfWork UnitOfWork;

    public DanhMucBuocUpdateCommandHandler(IServiceProvider serviceProvider) {
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        DanhMucManHinh = serviceProvider.GetRequiredService<IRepository<DanhMucManHinh, int>>();
        UnitOfWork = DanhMucBuoc.UnitOfWork;
    }

    public async Task<DanhMucBuoc> Handle(DanhMucBuocUpdateCommand request, CancellationToken cancellationToken) {
        using (await UnitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
            await ValidateAsync(request, cancellationToken);

            var entity = await DanhMucBuoc.GetOrderedSet()
                .Include(e => e.BuocManHinhs)
                .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
            ManagedException.ThrowIfNull(entity);

            DanhMucBuoc? parent = null;
            if (request.Dto.ParentId > 0) {
                parent = await DanhMucBuoc.GetOrderedSet().FirstOrDefaultAsync(c => c.Id == request.Dto.ParentId, cancellationToken: cancellationToken);
                ManagedException.ThrowIf(parent == null, "Bước cha không tồn tại.");
            }

            entity.Update(request.Dto);

            await DanhMucBuoc.MoveNodeAsync(entity, parent, cancellationToken);

            if (request.Dto.DanhSachManHinh?.Count > 0) {
                // Cập nhật BuocManHinhs với Stt từ request
                entity.BuocManHinhs = [ ..request.Dto.DanhSachManHinh.Select((manHinhId, index) => new DanhMucBuocManHinh() {
                    BuocId = entity.Id,
                    ManHinhId = manHinhId,
                    Stt = index
                }) ];
                
                var danhSachManHinh = await DanhMucManHinh.GetQueryableSet()
                    .Where(e => request.Dto.DanhSachManHinh.Contains(e.Id))
                    .ToListAsync(cancellationToken: cancellationToken);

                entity.PartialView = string.Join(";", danhSachManHinh.Select(e => e.Ten?.Trim()) ?? []);
            } else {
                entity.BuocManHinhs = [];
                entity.PartialView = string.Empty;
            }

            await UpdateAsync(entity, cancellationToken);

            await UnitOfWork.SaveChangesAsync(cancellationToken);
            await UnitOfWork.CommitTransactionAsync(cancellationToken);

            // Reload entity từ database để đảm bảo BuocManHinhs có Stt values chính xác
            // Điều này cần thiết vì EF Core có thể reload collection theo default order (Primary Key)
            var reloadedEntity = await DanhMucBuoc.GetOrderedSet()
                .Include(e => e.BuocManHinhs)
                .FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken);
            
            return reloadedEntity ?? entity;
        }
    }

    #region Private helper methods

    private Task ValidateAsync(DanhMucBuocUpdateCommand request, CancellationToken cancellationToken) {
        // Add validation logic if needed
        return Task.CompletedTask;
    }

    private async Task UpdateAsync(DanhMucBuoc entity, CancellationToken cancellationToken) {
        await DanhMucBuoc.UpdateAsync(entity, cancellationToken);
    }

    #endregion
}