using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.PheDuyetDuToans.Commands;

/// <summary>
/// Duyệt phê duyệt dự toán - chỉ BGĐ role
/// </summary>
public record PheDuyetDuToanDuyetCommand(Guid Id) : IRequest<int>;

internal class PheDuyetDuToanDuyetCommandHandler : IRequestHandler<PheDuyetDuToanDuyetCommand, int> {
    private readonly IRepository<PheDuyetDuToan, Guid> _repository;
    private readonly IRepository<PheDuyetDuToanHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyetDuToan, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public PheDuyetDuToanDuyetCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToanHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyetDuToan, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(PheDuyetDuToanDuyetCommand request, CancellationToken cancellationToken) {
        // Permission check: BGĐ role only
        if (!_userProvider.AuthInfo.HasRole("BGĐ")) {
            throw new ManagedException("Chỉ Ban Giám đốc có quyền duyệt phê duyệt dự toán");
        }

        // Get status IDs from DB by code
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetDuToanCodes.DaTrinh, cancellationToken);
        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetDuToanCodes.DaDuyet, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy phê duyệt dự toán");

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể duyệt khi trạng thái là Đã trình");
        }

        // Update status to Đã duyệt
        entity.TrangThaiId = trangThaiDaDuyet.Id;
        entity.NguoiGiaoViecId = _userProvider.Info.UserID;

        // Create history record
        var history = new PheDuyetDuToanHistory {
            Id = Guid.NewGuid(),
            PheDuyetDuToanId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaDuyet.Id,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}