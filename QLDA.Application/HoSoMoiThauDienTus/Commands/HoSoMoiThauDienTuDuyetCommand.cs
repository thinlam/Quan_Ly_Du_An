using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Enums;
using QLDA.Application.QuanLyPheDuyet.Commands;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

/// <summary>
/// Duyệt hồ sơ mời thầu điện tử - chỉ BGĐ role
/// </summary>
public record HoSoMoiThauDienTuDuyetCommand(Guid Id) : IRequest<int>;

internal class HoSoMoiThauDienTuDuyetCommandHandler : IRequestHandler<HoSoMoiThauDienTuDuyetCommand, int> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepo;
    private readonly IDapperRepository _dapper;
    private readonly IRepository<VanBanQuyetDinh, Guid> _quyetDinhRepo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public HoSoMoiThauDienTuDuyetCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _pheDuyetRepo = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(HoSoMoiThauDienTuDuyetCommand request, CancellationToken cancellationToken) {

       

        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaTrinh && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaDuyet && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        var entity = await _repository.GetQueryableSet()
            .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy hồ sơ mời thầu điện tử");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể duyệt khi trạng thái là Đã trình");
        }

        entity.TrangThaiId = trangThaiDaDuyet.Id;
        var VanBanQuyetDinh = new VanBanQuyetDinh {
            Id = entity.Id,
            So = entity.QuyetDinh?.So,
            TrichYeu = entity.QuyetDinh?.TrichYeu,
            NguoiKy = entity.QuyetDinh?.NguoiKy,
            Ngay = entity.QuyetDinh?.Ngay,
            NgayKy = entity.QuyetDinh?.NgayKy,
            DuAnId = entity.DuAnId ?? Guid.Empty,
            BuocId = entity.BuocId,
            Loai = EnumLoaiVanBanQuyetDinh.HoSoMoiThauDienTu.ToString(),
        };
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.HoSoMoiThauDienTu,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId ?? Guid.Empty,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaDuyet.Id,
            NgayXuLy = DateTimeOffset.UtcNow
        };
        await _quyetDinhRepo.AddAsync(VanBanQuyetDinh, cancellationToken);

        await _historyRepository.AddAsync(history, cancellationToken);

                var affected = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affected > 0) {
            await PheDuyetNotificationHelper.NotifyAfterSaveAsync(
                _dapper,
                _historyRepository,
                _pheDuyetRepo,
                _userProvider.Info.UserID,
                PheDuyetEntityNames.HoSoMoiThauDienTu,
                entity.Id,
                entity.DuAn?.TenDuAn,
                entity.CreatedBy,
                PheDuyetNotificationAction.Duyet,
                maTrangThaiTrinh: TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaTrinh,
                cancellationToken: cancellationToken);
        }

        return affected;
    }
}
