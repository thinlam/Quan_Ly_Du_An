using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.DuAnBuocs.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAnBuocs.Commands;

/// <summary>
/// Cập nhật bước dự án.
/// Phân quyền:
/// - Tất cả field (TenBuoc, Ngay, ManHinh, PhongPhuTrachChinhId, DanhSachPhongBanPhoiHopIds): chỉ Owner/LanhDao/KHTC
/// - DanhSachPhongBanPhoiHopIds: validate mọi ID phải thuộc DuAn.DuAnChiuTrachNhiemXuLys (Loai=DonViPhoiHop)
/// </summary>
public record DuAnBuocUpdateCommand(DuAnBuocUpdateDto Dto) : IRequest<DuAnBuoc>;

public record DuAnBuocUpdateCommandHandler : IRequestHandler<DuAnBuocUpdateCommand, DuAnBuoc> {
    private readonly IRepository<DuAnBuoc, int> DuAnBuoc;
    private readonly IRepository<DanhMucManHinh, int> DanhMucManHinh;
    private readonly IRepository<DuAn, Guid> _duAnRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _ctx;
    private readonly IUnitOfWork _unitOfWork;

    public DuAnBuocUpdateCommandHandler(IServiceProvider serviceProvider) {
        DuAnBuoc = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        DanhMucManHinh = serviceProvider.GetRequiredService<IRepository<DanhMucManHinh, int>>();
        _duAnRepo = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _ctx = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = DuAnBuoc.UnitOfWork;
    }

    public async Task<DuAnBuoc> Handle(DuAnBuocUpdateCommand request, CancellationToken cancellationToken) {
        var entity = await DuAnBuoc.GetQueryableSet()
                    .Include(e => e.DuAn)
                    .Include(e => e.DuAnBuocManHinhs)
                    .Include(e => e.DuAnBuocPhongBanPhoiHops)
                    .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Phân quyền: tất cả field đều yêu cầu Owner/LanhDao/KHTC/role thuộc GroupAdminCatalog.
        // Bypass GroupAdminCatalog đã được move vào EnsureCanManageStepFieldsAsync.
        await _auth.EnsureCanManageStepFieldsAsync(entity.Id, _ctx, cancellationToken);

        entity.Update(request.Dto);

        // Update PhongPhuTrachChinhId
        // Validate: phải thuộc DuAn.DuAnChiuTrachNhiemXuLys HOẶC trùng DuAn.DonViPhuTrachChinhId.
        if (request.Dto.PhongPhuTrachChinhId.HasValue) {
            var target = request.Dto.PhongPhuTrachChinhId.Value;
            var allowed = await _duAnRepo.GetQueryableSet()
                .Where(d => d.Id == entity.DuAnId)
                .Select(d => new {
                    d.DonViPhuTrachChinhId,
                    InChiuTrachNhiem = d.DuAnChiuTrachNhiemXuLys!.Any(x => x.RightId == target)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var matches = allowed is not null
                && (allowed.DonViPhuTrachChinhId == target || allowed.InChiuTrachNhiem);

            if (!matches)
                throw new ManagedException(
                    $"Phòng ban phụ trách chính ({target}) không thuộc phạm vi chịu trách nhiệm xử lý của dự án.");
        }

        entity.PhongPhuTrachChinhId = request.Dto.PhongPhuTrachChinhId;

        // Update DuAnBuocPhongBanPhoiHops
        // - null  : không thay đổi
        // - []    : xoá hết
        // - [ids] : validate + replace
        request.Dto.DanhSachPhongBanPhoiHopIds ??= [];

        // (1) Validate IDs thuộc DuAn.DuAnChiuTrachNhiemXuLys (Loai=DonViPhoiHop)
        //     HOẶC trùng DuAn.DonViPhuTrachChinhId (phòng ban phụ trách chính cũng được phép thêm vào danh sách phối hợp).
        //     Bỏ qua khi rỗng.
        var allowedPhongBanInfo = await _duAnRepo.GetQueryableSet()
            .Where(d => d.Id == entity.DuAnId)
            .Select(d => new {
                PhoiHopIds = d.DuAnChiuTrachNhiemXuLys!
                    .Where(x => x.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop)
                    .Select(x => x.RightId),
                d.DonViPhuTrachChinhId
            })
            .FirstOrDefaultAsync(cancellationToken);

        var allowedSet = new HashSet<long>(allowedPhongBanInfo?.PhoiHopIds ?? []);
        if (allowedPhongBanInfo?.DonViPhuTrachChinhId.HasValue == true)
            allowedSet.Add(allowedPhongBanInfo.DonViPhuTrachChinhId.Value);

        var invalid = request.Dto.DanhSachPhongBanPhoiHopIds
            .Where(id => !allowedSet.Contains(id))
            .ToList();

        if (invalid.Count > 0)
            throw new ManagedException(
                $"Các phòng ban sau không thuộc phạm vi chịu trách nhiệm xử lý của dự án: {string.Join(", ", invalid)}");

        // (2) Replace collection (Clear() + Add — empty list = clear all)
        entity.DuAnBuocPhongBanPhoiHops ??= [];
        entity.DuAnBuocPhongBanPhoiHops.Clear();
        foreach (var phongBanId in request.Dto.DanhSachPhongBanPhoiHopIds) {
            entity.DuAnBuocPhongBanPhoiHops.Add(new DuAnBuocPhongBanPhoiHop {
                LeftId = entity.Id,
                RightId = phongBanId
            });
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
