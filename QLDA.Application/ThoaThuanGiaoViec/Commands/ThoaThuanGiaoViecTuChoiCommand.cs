using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ThoaThuanGiaoViecs.Commands;

/// <summary>
/// Trả lại phân khai kinh phí - LDDV role, cần lý do
/// </summary>
public record ThoaThuanGiaoViecTuChoiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class ThoaThuanGiaoViecTuChoiCommandHandler : IRequestHandler<ThoaThuanGiaoViecTuChoiCommand, int> {
    private readonly IRepository<Domain.Entities.ThoaThuanGiaoViec, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public ThoaThuanGiaoViecTuChoiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.ThoaThuanGiaoViec, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ThoaThuanGiaoViecTuChoiCommand request, CancellationToken cancellationToken) {
        if (!_userProvider.AuthInfo.HasRole(Domain.Constants.RoleConstants.QLDA_LDDV) )
        {
            throw new ManagedException("Tài khoản không có quyền.");
        }

        // Validate NoiDung is required
        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do từ chối là bắt buộc");
        }

        // Get status IDs from DB by code
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.KeHoachLuaChonNhaThauRutGon.DaTrinh
            && s.Loai == PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);
        var trangThaiTuChoi = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.KeHoachLuaChonNhaThauRutGon.TuChoi 
            && s.Loai == PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể trả lại khi trạng thái là Đã trình");
        }

        // Update status to Trả lại
        entity.TrangThaiId = trangThaiTuChoi.Id;

        // Create history record with reason
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.ThoaThuanGiaoViec,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTuChoi.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}