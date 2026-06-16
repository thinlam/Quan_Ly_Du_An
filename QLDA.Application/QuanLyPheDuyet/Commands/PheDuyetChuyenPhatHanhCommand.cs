using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.QuanLyPheDuyet.Commands;

/// <summary>
/// Chuyen P.HC-TH de phat hanh so (UC22 step 5-6, issue #9459)
/// </summary>
public record PheDuyetChuyenPhatHanhCommand(string Type, Guid Id, string? SoPhatHanh = null) : IRequest<int>;

internal class PheDuyetChuyenPhatHanhCommandHandler : IRequestHandler<PheDuyetChuyenPhatHanhCommand, int> {
    private readonly IRepository<PheDuyetDuToan, Guid> _duToanRepo;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public PheDuyetChuyenPhatHanhCommandHandler(IServiceProvider serviceProvider) {
        _duToanRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _historyRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _duToanRepo.UnitOfWork;
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
    }

    public async Task<int> Handle(PheDuyetChuyenPhatHanhCommand request, CancellationToken cancellationToken) {
        // Permission: P.HC-TH (by PhongBanID from appsettings) or BGĐ
        var isHcth = _userProvider.Info.PhongBanID == _settings.PhongHCTHID;
        var isBgd = _userProvider.AuthInfo?.HasRole(QLDA.Domain.Constants.RoleConstants.QLDA_LDDV) ?? false;
        if (!isHcth && !isBgd) {
            throw new ManagedException("Chỉ P.HC-TH hoặc BGĐ có quyền phát hành");
        }

        // Get DaDuyet status to validate entity is approved
        var trangThaiDaDuyet = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DuToan.DaDuyet && s.Loai == PheDuyetEntityNames.PheDuyetDuToan, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        // Get entity based on type
        var (entity, duAnId) = request.Type switch {
            PheDuyetEntityNames.PheDuyetDuToan => await GetDuToanEntity(request.Id, cancellationToken),
            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };

        // Validate current status must be DaDuyet
        if (entity.TrangThaiId != trangThaiDaDuyet.Id) {
            throw new ManagedException("Chỉ có thể phát hành khi trạng thái là Đã duyệt");
        }

        if (entity.BuocId.HasValue) {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == entity.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _userProvider, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }

        // Update SoPhatHanh if provided
        if (!string.IsNullOrWhiteSpace(request.SoPhatHanh)) {
            entity.So = request.SoPhatHanh;
        }

        // Create history record
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = request.Type,
            EntityId = entity.Id,
            DuAnId = duAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaDuyet.Id,
            NoiDung = $"Phát hành: {request.SoPhatHanh}",
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepo.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<(PheDuyetDuToan Entity, Guid DuAnId)> GetDuToanEntity(Guid id, CancellationToken cancellationToken) {
        var entity = await _duToanRepo.GetQueryableSet()
            .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy phê duyệt dự toán");
        return (entity, entity.DuAnId);
    }
}
