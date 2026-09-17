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
                var danhSachManHinh = await DanhMucManHinh.GetQueryableSet()
                    .Where(e => request.Dto.DanhSachManHinh.Contains(e.Id))
                    .ToListAsync(cancellationToken: cancellationToken);

                var sorted = danhSachManHinh.OrderBy(e => request.Dto.DanhSachManHinh.IndexOf(e.Id));
                entity.PartialView = string.Join(";", sorted.Select(e => e.Ten?.Trim()));

                var sortedIds = sorted.Select(e => e.Id).ToList();
                foreach (var mh in entity.BuocManHinhs ?? []) {
                    mh.Stt = sortedIds.IndexOf(mh.RightId) + 1;
                }
            }

            await UpdateAsync(entity, cancellationToken);

            await UnitOfWork.SaveChangesAsync(cancellationToken);
            await UnitOfWork.CommitTransactionAsync(cancellationToken);

            return entity;
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