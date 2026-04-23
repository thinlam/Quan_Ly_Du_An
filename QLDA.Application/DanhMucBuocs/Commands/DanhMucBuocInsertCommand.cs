using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucBuocs.DTOs;

namespace QLDA.Application.DanhMucBuocs.Commands;

public record DanhMucBuocInsertCommand(DanhMucBuocInsertDto Dto) : IRequest<DanhMucBuoc>;

internal class DanhMucBuocInsertCommandHandler : IRequestHandler<DanhMucBuocInsertCommand, DanhMucBuoc> {
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<DanhMucManHinh, int> DanhMucManHinh;
    private readonly IUnitOfWork UnitOfWork;

    public DanhMucBuocInsertCommandHandler(IServiceProvider serviceProvider) {
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        DanhMucManHinh = serviceProvider.GetRequiredService<IRepository<DanhMucManHinh, int>>();
        UnitOfWork = DanhMucBuoc.UnitOfWork;
    }

    public async Task<DanhMucBuoc> Handle(DanhMucBuocInsertCommand request, CancellationToken cancellationToken) {
        await ValidateAsync(request, cancellationToken);

        var entity = request.Dto.ToEntity();

        if (UnitOfWork.HasTransaction) {
            await InsertAsync(request, entity, cancellationToken);
        } else {
            using var tx = await UnitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await InsertAsync(request, entity, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);
            await UnitOfWork.CommitTransactionAsync(cancellationToken);
        }


        return entity;
    }

    #region Private helper methods

    private static Task ValidateAsync(DanhMucBuocInsertCommand request, CancellationToken cancellationToken) {
        // Add validation logic if needed
        return Task.CompletedTask;
    }

    private async Task<DanhMucBuoc> InsertAsync(DanhMucBuocInsertCommand request, DanhMucBuoc entity, CancellationToken cancellationToken) {

        await DanhMucBuoc.AddAsync(entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        DanhMucBuoc? parent = null;
        if (request.Dto.ParentId > 0) {
            parent = await DanhMucBuoc.GetOrderedSet().FirstOrDefaultAsync(c => c.Id == request.Dto.ParentId, cancellationToken: cancellationToken);
            ManagedException.ThrowIf(parent == null, "Bước cha không tồn tại.");
        }

        // Cập nhật BuocManHinhs với BuocId và Stt từ request
        if (request.Dto.DanhSachManHinh?.Count > 0) {
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
        DanhMucBuoc.InitializeNode(entity, parent);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        
        // Reload entity từ database để đảm bảo BuocManHinhs có Stt values chính xác
        // Điều này cần thiết vì EF Core có thể reload collection theo default order (Primary Key)
        var reloadedEntity = await DanhMucBuoc.GetOrderedSet()
            .Include(e => e.BuocManHinhs)
            .FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken);
        
        return reloadedEntity ?? entity;
    }

    #endregion
}
