using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.DuAnBuocs.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAnBuocs.Commands;

/// <summary>
/// Cập nhật trạng thái bước dự án + cho phép cập nhật PhongPhuTrachChinhId, DanhSachPhongBanPhoiHopIds.
/// Phân quyền:
/// - Tất cả field (TrangThaiId, NgayDuKien, NgayThucTe, GhiChu, TrachNhiemThucHien, IsKetThuc, PhongPhuTrachChinhId, DanhSachPhongBanPhoiHopIds):
///   chỉ Owner (CreatedBy) + Lãnh đạo phụ trách (DuAn.LanhDaoPhuTrachId) + role thuộc GroupAdminCatalog (QLDA_QuanTri / QLDA_TatCa) hoặc Phòng KHTC (HasKhtcBypass).
/// - DanhSachPhongBanPhoiHopIds: validate mọi ID phải thuộc DuAn.DuAnChiuTrachNhiemXuLys (Loai=DonViPhoiHop).
/// </summary>
public record DuAnBuocDuAnUpdateStateCommand(DuAnBuocDuAnUpdateStateDto Dto) : IRequest<DuAnBuoc>;

public record DuAnBuocDuAnUpdateStateCommandHandler : IRequestHandler<DuAnBuocDuAnUpdateStateCommand, DuAnBuoc> {
    private readonly IRepository<DuAnBuoc, int> DuAnBuoc;
    private readonly IRepository<DuAn, Guid> _duAnRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _ctx;
    private readonly IUnitOfWork _unitOfWork;

    public DuAnBuocDuAnUpdateStateCommandHandler(IServiceProvider serviceProvider) {
        DuAnBuoc = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _duAnRepo = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _ctx = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = DuAnBuoc.UnitOfWork;
    }

    public async Task<DuAnBuoc> Handle(DuAnBuocDuAnUpdateStateCommand request, CancellationToken cancellationToken) {
        var entity = await DuAnBuoc.GetQueryableSet()
            .Include(e => e.DuAn)
            .Include(e => e.DuAnBuocPhongBanPhoiHops)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Phân quyền: tất cả field đều yêu cầu Owner/LanhDao/KHTC
        await _auth.EnsureCanManageStepFieldsAsync(entity.Id, _ctx, cancellationToken);

        // Validate PhongPhuTrachChinhId (nếu có) thuộc DuAn.DuAnChiuTrachNhiemXuLys hoặc DuAn.DonViPhuTrachChinhId
        if (request.Dto.PhongPhuTrachChinhId.HasValue) {
            var allowedPhongBanIds = await _duAnRepo.GetQueryableSet()
                .Where(d => d.Id == entity.DuAnId)
                .Select(d => new {
                    ChiuTrachNhiemIds = d.DuAnChiuTrachNhiemXuLys!.Select(x => x.RightId),
                    d.DonViPhuTrachChinhId
                })
                .Select(d => new { All = d.ChiuTrachNhiemIds.Concat(new[] { d.DonViPhuTrachChinhId ?? 0 }) })
                .FirstOrDefaultAsync(cancellationToken);

            var allowedSet = (allowedPhongBanIds?.All ?? Enumerable.Empty<long>()).ToHashSet();
            if (!allowedSet.Contains(request.Dto.PhongPhuTrachChinhId.Value))
                throw new ManagedException(
                    $"Phòng ban phụ trách chính ({request.Dto.PhongPhuTrachChinhId}) không thuộc phạm vi chịu trách nhiệm xử lý của dự án.");
        }

        entity.UpdateState(request.Dto);

        // Update DanhSachPhongBanPhoiHops (nếu DTO gửi list)
        if (request.Dto.DanhSachPhongBanPhoiHopIds != null) {
            // (1) Validate IDs thuộc DuAn.DuAnChiuTrachNhiemXuLys (Loai=DonViPhoiHop)
            var allowedPhongBanIds = await _duAnRepo.GetQueryableSet()
                .Where(d => d.Id == entity.DuAnId)
                .SelectMany(d => d.DuAnChiuTrachNhiemXuLys!
                    .Where(x => x.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop)
                    .Select(x => x.RightId))
                .ToListAsync(cancellationToken);

            var invalid = request.Dto.DanhSachPhongBanPhoiHopIds
                .Where(id => !allowedPhongBanIds.Contains(id))
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
