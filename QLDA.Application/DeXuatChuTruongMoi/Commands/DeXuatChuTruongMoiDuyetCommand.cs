using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.DeXuatChuTruongMois.Commands;

/// <summary>
/// Duyệt phân khai kinh phí - LDDV role
/// </summary>
public record DeXuatChuTruongMoiDuyetCommand(Guid Id) : IRequest<int>;

internal class DeXuatChuTruongMoiDuyetCommandHandler : IRequestHandler<DeXuatChuTruongMoiDuyetCommand, int> {
    private readonly IRepository<Domain.Entities.DeXuatChuTruongMoi, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatChuTruongMoiDuyetCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.DeXuatChuTruongMoi, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatChuTruongMoiDuyetCommand request, CancellationToken cancellationToken) {
        // Permission check: LDDV role only
        if (!_userProvider.AuthInfo.HasRole(Domain.Constants.RoleConstants.QLDA_LDDV)) {
            throw new ManagedException("Tài khoản không có quyền.");
        }

        // Get status IDs from DB by code
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatChuTruongMoi.DaTrinh && s.Loai == PheDuyetEntityNames.DeXuatChuTruongMoi, cancellationToken);
        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatChuTruongMoi.DaDuyet && s.Loai == PheDuyetEntityNames.DeXuatChuTruongMoi, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể duyệt khi trạng thái là Đã trình");
        }

        // Update status to Đã duyệt
        entity.TrangThaiId = trangThaiDaDuyet.Id;

        // Create history record
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.DeXuatChuTruongMoi,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaDuyet.Id,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}