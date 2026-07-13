using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.PheDuyetDuToans.Commands;

/// <summary>
/// Trình phê duyệt dự toán - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record PheDuyetDuToanTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class PheDuyetDuToanTrinhCommandHandler : IRequestHandler<PheDuyetDuToanTrinhCommand, int> {
    private readonly IRepository<PheDuyetDuToan, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public PheDuyetDuToanTrinhCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(PheDuyetDuToanTrinhCommand request, CancellationToken cancellationToken) {

        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DuToan.DuThao && s.Loai == PheDuyetEntityNames.PheDuyetDuToan, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DuToan.TraLai && s.Loai == PheDuyetEntityNames.PheDuyetDuToan, cancellationToken);
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DuToan.DaTrinh && s.Loai == PheDuyetEntityNames.PheDuyetDuToan, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy phê duyệt dự toán");

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        // Validate current status must be null (legacy), Dự thảo, or Trả lại
        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id) {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là Dự thảo");
        }

        // Update status to Đã trình
        entity.TrangThaiId = trangThaiDaTrinh.Id;
        entity.NguoiXuLyId = _userProvider.Info.UserID;

        // Create history record
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.PheDuyetDuToan,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
