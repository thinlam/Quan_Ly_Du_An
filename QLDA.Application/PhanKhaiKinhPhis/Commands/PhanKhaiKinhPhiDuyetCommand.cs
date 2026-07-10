using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Application.QuanLyPheDuyet.Commands;

namespace QLDA.Application.PhanKhaiKinhPhis.Commands;

/// <summary>
/// Duyệt phân khai kinh phí - LDDV role
/// </summary>
public record PhanKhaiKinhPhiDuyetCommand(Guid Id) : IRequest<int>;

internal class PhanKhaiKinhPhiDuyetCommandHandler : IRequestHandler<PhanKhaiKinhPhiDuyetCommand, int> {
    private readonly IRepository<Domain.Entities.PhanKhaiKinhPhi, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepo;
    private readonly IDapperRepository _dapper;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public PhanKhaiKinhPhiDuyetCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.PhanKhaiKinhPhi, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _pheDuyetRepo = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(PhanKhaiKinhPhiDuyetCommand request, CancellationToken cancellationToken) {
        // Permission check: LDDV role only

        // Get status IDs from DB by code
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DaTrinh && s.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi, cancellationToken);
        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DaDuyet && s.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        var entity = await _repository.GetQueryableSet()
            .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy phân khai kinh phí");

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể duyệt khi trạng thái là Đã trình");
        }

        // Update status to Đã duyệt
        entity.TrangThaiId = trangThaiDaDuyet.Id;

        // Create history record
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.PhanKhaiKinhPhi,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaDuyet.Id,
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
                PheDuyetEntityNames.PhanKhaiKinhPhi,
                entity.Id,
                entity.DuAn?.TenDuAn,
                entity.CreatedBy,
                PheDuyetNotificationAction.Duyet,
                maTrangThaiTrinh: TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DaTrinh,
                cancellationToken: cancellationToken);
        }

        return affected;
    }
}