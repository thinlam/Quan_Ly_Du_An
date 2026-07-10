using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Application.QuanLyPheDuyet.Commands;

namespace QLDA.Application.PheDuyetDuToans.Commands;

/// <summary>
/// Trả lại phê duyệt dự toán - chỉ BGĐ role, cần lý do
/// </summary>
public record PheDuyetDuToanTraLaiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class PheDuyetDuToanTraLaiCommandHandler : IRequestHandler<PheDuyetDuToanTraLaiCommand, int> {
    private readonly IRepository<PheDuyetDuToan, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepo;
    private readonly IDapperRepository _dapper;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public PheDuyetDuToanTraLaiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _pheDuyetRepo = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(PheDuyetDuToanTraLaiCommand request, CancellationToken cancellationToken) {

        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do trả lại là bắt buộc");
        }

        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DuToan.DaTrinh && s.Loai == PheDuyetEntityNames.PheDuyetDuToan, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DuToan.TraLai && s.Loai == PheDuyetEntityNames.PheDuyetDuToan, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiTraLai, "Không tìm thấy trạng thái 'Trả lại'");

        var entity = await _repository.GetQueryableSet()
            .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy phê duyệt dự toán");

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể trả lại khi trạng thái là Đã trình");
        }

        // Update status to Trả lại
        entity.TrangThaiId = trangThaiTraLai.Id;

        // Create history record with reason
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.PheDuyetDuToan,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTraLai.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

                var affected = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affected > 0) {
            await PheDuyetNotificationHelper.NotifyAfterSaveAsync(
                _dapper,
                _historyRepository,
                _pheDuyetRepo,
                _userProvider.Info.UserID,
                PheDuyetEntityNames.PheDuyetDuToan,
                entity.Id,
                entity.DuAn?.TenDuAn,
                entity.CreatedBy,
                PheDuyetNotificationAction.TraLai,
                request.NoiDung,
                maTrangThaiTrinh: TrangThaiPheDuyetCodes.DuToan.DaTrinh,
                cancellationToken: cancellationToken);
        }

        return affected;
    }
}