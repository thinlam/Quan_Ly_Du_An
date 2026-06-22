using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

/// <summary>
/// Trả lại hồ sơ mời thầu điện tử - chỉ BGĐ role, cần lý do
/// </summary>
public record HoSoMoiThauDienTuTraLaiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class HoSoMoiThauDienTuTraLaiCommandHandler : IRequestHandler<HoSoMoiThauDienTuTraLaiCommand, int> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;


    public HoSoMoiThauDienTuTraLaiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(HoSoMoiThauDienTuTraLaiCommand request, CancellationToken cancellationToken) {
         var phongBanId = _userProvider.Info.PhongBanID;
        if (!_userProvider.AuthInfo.HasRole(Domain.Constants.RoleConstants.QLDA_LDDV) && phongBanId != _settings.PhongHCTHId)
        {
            throw new ManagedException("Tài khoản không có quyền.");
        }

        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do trả lại là bắt buộc");
        }

        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaTrinh && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.TraLai && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiTraLai, "Không tìm thấy trạng thái 'Trả lại'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy hồ sơ mời thầu điện tử");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể trả lại khi trạng thái là Đã trình");
        }

        entity.TrangThaiId = trangThaiTraLai.Id;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.HoSoMoiThauDienTu,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId ?? Guid.Empty,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTraLai.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
