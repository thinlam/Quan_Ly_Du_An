using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.DeXuatChuyenTieps.Commands;

/// <summary>
/// Duyệt phân khai kinh phí - LDDV role
/// </summary>
public record DeXuatChuyenTiepDuyetCommand(Guid Id) : IRequest<int>;

internal class DeXuatChuyenTiepDuyetCommandHandler : IRequestHandler<DeXuatChuyenTiepDuyetCommand, int> {
    private readonly IRepository<Domain.Entities.DeXuatChuyenTiep, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public DeXuatChuyenTiepDuyetCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.DeXuatChuyenTiep, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatChuyenTiepDuyetCommand request, CancellationToken cancellationToken) {
        var isHcth = _userProvider.Info.PhongBanID == _settings.PhongHCTHID;
        if (!_userProvider.AuthInfo.HasRole(Domain.Constants.RoleConstants.QLDA_LDDV) && !isHcth)
        {
            throw new ManagedException("Tài khoản không có quyền.");
        }
        // Get status IDs from DB by code
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh && s.Loai == PheDuyetEntityNames.DeXuatChuTruongChuyenTiep, cancellationToken);
        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet && s.Loai == PheDuyetEntityNames.DeXuatChuTruongChuyenTiep, cancellationToken);

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
            EntityName = PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
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