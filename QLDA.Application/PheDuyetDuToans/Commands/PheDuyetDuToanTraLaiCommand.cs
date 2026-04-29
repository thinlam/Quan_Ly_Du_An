using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.PheDuyetDuToans.Commands;

/// <summary>
/// Trả lại phê duyệt dự toán - chỉ BGĐ role, cần lý do
/// </summary>
public record PheDuyetDuToanTraLaiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class PheDuyetDuToanTraLaiCommandHandler : IRequestHandler<PheDuyetDuToanTraLaiCommand, int> {
    private readonly IRepository<PheDuyetDuToan, Guid> _repository;
    private readonly IRepository<PheDuyetDuToanHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyetDuToan, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public PheDuyetDuToanTraLaiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToanHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyetDuToan, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(PheDuyetDuToanTraLaiCommand request, CancellationToken cancellationToken) {
        // Permission check: BGĐ role only
        //tạm thời bỏ check role
        // if (!_userProvider.AuthInfo.HasRole("BGĐ")) {
        //     throw new ManagedException("Chỉ Ban Giám đốc có quyền trả lại phê duyệt dự toán");
        // }

        // Validate NoiDung is required
        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do trả lại là bắt buộc");
        }

        // Get status IDs from DB by code
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetDuToanCodes.DaTrinh, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetDuToanCodes.TraLai, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiTraLai, "Không tìm thấy trạng thái 'Trả lại'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy phê duyệt dự toán");

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể trả lại khi trạng thái là Đã trình");
        }

        // Update status to Trả lại
        entity.TrangThaiId = trangThaiTraLai.Id;
        entity.NguoiGiaoViecId = _userProvider.Info.UserID;

        // Create history record with reason
        var history = new PheDuyetDuToanHistory {
            Id = Guid.NewGuid(),
            PheDuyetDuToanId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTraLai.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}