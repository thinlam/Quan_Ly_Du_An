using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.DuAnBuocs.DTOs;
using QLDA.Domain.Enums;
using RoleConstants = QLDA.Domain.Constants.RoleConstants;

namespace QLDA.Application.DuAnBuocs.Commands;

/// <summary>
/// Cập nhật trạng thái bước dự án + cho phép cập nhật PhongPhuTrachChinhId, DanhSachPhongBanPhoiHopIds.
/// Phân quyền (đúng theo doc-class):
/// - Owner (CreatedBy) + Lãnh đạo phụ trách (DuAn.LanhDaoPhuTrachId) + role thuộc GroupAdminCatalog (QLDA_QuanTri / QLDA_TatCa)
///   + Phòng KHTC (HasKhtcBypass) + Phòng ban phụ trách chính của DuAn (DuAn.DonViPhuTrachChinhId == user.PhongBanId).
/// - DanhSachPhongBanPhoiHopIds: validate mọi ID phải thuộc DuAn.DuAnChiuTrachNhiemXuLys (Loai=DonViPhoiHop) HOẶC DuAn.DonViPhuTrachChinhId.
/// </summary>
public record DuAnBuocDuAnUpdateStateCommand(DuAnBuocDuAnUpdateStateDto Dto) : IRequest<DuAnBuoc>;

internal class DuAnBuocDuAnUpdateStateCommandHandler(
    IRepository<DuAnBuoc, int> _duAnBuoc,
    IRepository<DuAn, Guid> _duAnRepo,
    IBuocAuthorizationProvider _auth,
    IAuthorizationContext _ctx,
    IUnitOfWork _unitOfWork)
    : IRequestHandler<DuAnBuocDuAnUpdateStateCommand, DuAnBuoc> {

    public async Task<DuAnBuoc> Handle(DuAnBuocDuAnUpdateStateCommand request, CancellationToken cancellationToken) {
        var entity = await _duAnBuoc.GetQueryableSet()
            .Include(e => e.DuAn)
            .Include(e => e.DuAnBuocPhongBanPhoiHops)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Phân quyền theo doc-class:
        // - KHTC bypass
        // - QLDA_QuanTri / QLDA_TatCa (GroupAdminCatalog)
        // - Lãnh đạo phụ trách DuAn (DuAn.LanhDaoPhuTrachId == userId)
        // - Owner (CreatedBy == userId.ToString())
        // - Phòng ban phụ trách chính của DuAn (DuAn.DonViPhuTrachChinhId == user.PhongBanId)
        await _auth.EnsureCanExecuteStepAsync(entity.Id, _ctx, cancellationToken);

        // Validate PhongPhuTrachChinhId (nếu có) thuộc DuAn.DuAnChiuTrachNhiemXuLys hoặc DuAn.DonViPhuTrachChinhId
        if (request.Dto.PhongPhuTrachChinhId.HasValue) {
            var allowedPhongBanInfo = await _duAnRepo.GetQueryableSet()
                .Where(d => d.Id == entity.DuAnId)
                .Select(d => new {
                    ChiuTrachNhiemIds = d.DuAnChiuTrachNhiemXuLys!.Select(x => x.RightId),
                    d.DonViPhuTrachChinhId
                })
                .FirstOrDefaultAsync(cancellationToken);

            var allowedSet = (allowedPhongBanInfo?.ChiuTrachNhiemIds ?? Enumerable.Empty<long>()).ToHashSet();
            if (allowedPhongBanInfo?.DonViPhuTrachChinhId.HasValue == true)
                allowedSet.Add(allowedPhongBanInfo.DonViPhuTrachChinhId.Value);

            if (!allowedSet.Contains(request.Dto.PhongPhuTrachChinhId.Value))
                throw new ManagedException(
                    $"Phòng ban phụ trách chính ({request.Dto.PhongPhuTrachChinhId}) không thuộc phạm vi chịu trách nhiệm xử lý của dự án.");
        }

        entity.UpdateState(request.Dto);

        // Update DanhSachPhongBanPhoiHops (nếu DTO gửi list)
        if (request.Dto.DanhSachPhongBanPhoiHopIds != null) {
            // (1) Validate IDs thuộc DuAn.DuAnChiuTrachNhiemXuLys (Loai=DonViPhoiHop)
            //     HOẶC trùng DuAn.DonViPhuTrachChinhId (phòng ban phụ trách chính cũng được phép thêm vào danh sách phối hợp).
            var allowedPhongBanInfo = await _duAnRepo.GetQueryableSet()
                .Where(d => d.Id == entity.DuAnId)
                .Select(d => new {
                    PhoiHopIds = d.DuAnChiuTrachNhiemXuLys!
                        .Where(x => x.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop)
                        .Select(x => x.RightId),
                    d.DonViPhuTrachChinhId
                })
                .FirstOrDefaultAsync(cancellationToken);

            var allowedSet = new HashSet<long>(allowedPhongBanInfo?.PhoiHopIds ?? Enumerable.Empty<long>());
            if (allowedPhongBanInfo?.DonViPhuTrachChinhId.HasValue == true)
                allowedSet.Add(allowedPhongBanInfo.DonViPhuTrachChinhId.Value);

            var invalid = request.Dto.DanhSachPhongBanPhoiHopIds
                .Where(id => !allowedSet.Contains(id))
                .ToList();

            if (invalid.Count > 0)
                throw new ManagedException(
                    $"Các phòng ban sau không thuộc phạm vi chịu trách nhiệm xử lý của dự án: {string.Join(", ", invalid)}");

            // (2) Replace collection
            entity.DuAnBuocPhongBanPhoiHops?.Clear();
            foreach (var phongBanId in request.Dto.DanhSachPhongBanPhoiHopIds) {
                entity.DuAnBuocPhongBanPhoiHops!.Add(new DuAnBuocPhongBanPhoiHop {
                    LeftId = entity.Id,
                    RightId = phongBanId
                });
            }
        }

        if (_unitOfWork.HasTransaction) {
            await _duAnBuoc.UpdateAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await _duAnBuoc.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        return entity!;
    }
}
