using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuAnBuocs.DTOs;

namespace QLDA.Application.DuAnBuocs.Commands;

/// <summary>
/// Cập nhật bước dự án
/// </summary>
public record DuAnBuocUpdateCommand(DuAnBuocUpdateDto Dto) : IRequest<DuAnBuoc>;

public record DuAnBuocUpdateCommandHandler : IRequestHandler<DuAnBuocUpdateCommand, DuAnBuoc> {
    private readonly IRepository<DuAnBuoc, int> DuAnBuoc;
    private readonly IRepository<DanhMucManHinh, int> DanhMucManHinh;
    private readonly IUnitOfWork _unitOfWork;

    public DuAnBuocUpdateCommandHandler(IServiceProvider serviceProvider) {
        DuAnBuoc = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        DanhMucManHinh = serviceProvider.GetRequiredService<IRepository<DanhMucManHinh, int>>();
        _unitOfWork = DuAnBuoc.UnitOfWork;
    }

    public async Task<DuAnBuoc> Handle(DuAnBuocUpdateCommand request, CancellationToken cancellationToken) {
        var entity = await DuAnBuoc.GetQueryableSet()
                    .Include(e => e.DuAnBuocManHinhs)
                    .Include(e => e.DuAnBuocPhongBanPhoiHops)
                    .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.Update(request.Dto);

        // Update PhongPhuTrachChinhId
        entity.PhongPhuTrachChinhId = request.Dto.PhongPhuTrachChinhId;

        // Update DuAnBuocPhongBanPhoiHops
        if (request.Dto.DanhSachPhongBanPhoiHopIds != null) {
            // Remove existing
            entity.DuAnBuocPhongBanPhoiHops?.Clear();
            // Add new
            foreach (var phongBanId in request.Dto.DanhSachPhongBanPhoiHopIds) {
                entity.DuAnBuocPhongBanPhoiHops!.Add(new DuAnBuocPhongBanPhoiHop {
                    LeftId = entity.Id,
                    RightId = phongBanId
                });
            }
        }

        if (request.Dto.DanhSachManHinh?.Count > 0) {
            var danhSachManHinh = await DanhMucManHinh.GetQueryableSet().AsNoTracking()
                .Where(e => request.Dto.DanhSachManHinh.Contains(e.Id))
                .ToListAsync(cancellationToken: cancellationToken);

            // Preserve user's intended order from DTO
            var sorted = danhSachManHinh
                .OrderBy(e => request.Dto.DanhSachManHinh.IndexOf(e.Id))
                .ToList();

            // Preserve user's array order as-is
            entity.PartialView = string.Join(";", sorted.Select(e => e.Ten?.Trim()));

            var sortedIds = sorted.Select(e => e.Id).ToList();
            foreach (var mh in entity.DuAnBuocManHinhs ?? []) {
                mh.Stt = sortedIds.IndexOf(mh.RightId) + 1;
            }
        }

        if (_unitOfWork.HasTransaction) {
            await DuAnBuoc.UpdateAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await DuAnBuoc.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        return entity;
    }
}