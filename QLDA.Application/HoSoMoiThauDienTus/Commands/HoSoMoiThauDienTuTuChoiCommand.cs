using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

/// <summary>
/// Từ chối hồ sơ mời thầu điện tử - tất cả roles quản lý, cần lý do
/// </summary>
public record HoSoMoiThauDienTuTuChoiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class HoSoMoiThauDienTuTuChoiCommandHandler : IRequestHandler<HoSoMoiThauDienTuTuChoiCommand, int> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuTuChoiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(HoSoMoiThauDienTuTuChoiCommand request, CancellationToken cancellationToken) {
        var roles = _userProvider.AuthInfo?.Roles ?? [];
        if (!roles.Contains(Domain.Constants.RoleConstants.QLDA_LDDV) && !roles.Contains(Domain.Constants.RoleConstants.QLDA_HC_TH) && !roles.Contains(Domain.Constants.RoleConstants.QLDA_QuanTri)) {
            throw new ManagedException("Chỉ quản lý có quyền từ chối hồ sơ mời thầu điện tử");
        }

        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do từ chối là bắt buộc");
        }

        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaTrinh && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiTuChoi = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.TuChoi && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiTuChoi, "Không tìm thấy trạng thái 'Từ chối'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy hồ sơ mời thầu điện tử");

        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể từ chối khi trạng thái là Đã trình");
        }

        entity.TrangThaiId = trangThaiTuChoi.Id;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.HoSoMoiThauDienTu,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId ?? Guid.Empty,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTuChoi.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
