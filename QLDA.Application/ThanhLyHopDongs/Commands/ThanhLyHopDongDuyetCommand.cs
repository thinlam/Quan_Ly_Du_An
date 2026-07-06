using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ThanhLyHopDongs.Commands;

/// <summary>
/// UC63 — Duyệt thanh lý hợp đồng. Yêu cầu role QLDA_LDDV hoặc thuộc phòng HC-TH.
/// </summary>
public record ThanhLyHopDongDuyetCommand(Guid Id) : IRequest<int>;

internal class ThanhLyHopDongDuyetCommandHandler : IRequestHandler<ThanhLyHopDongDuyetCommand, int>
{
    private readonly IRepository<ThanhLyHopDong, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IAppSettingsProvider _settings;
    private readonly IUnitOfWork _unitOfWork;

    public ThanhLyHopDongDuyetCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<ThanhLyHopDong, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ThanhLyHopDongDuyetCommand request, CancellationToken cancellationToken)
    {
        var isHcth = _userProvider.Info.PhongBanID == _settings.PhongHCTHId;
        if (!_userProvider.AuthInfo.HasRole(QLDA.Domain.Constants.RoleConstants.QLDA_LDDV) && !isHcth)
        {
            throw new ManagedException("Tài khoản không có quyền.");
        }

        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.ThanhLyHopDong, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.ThanhLyHopDong.DaTrinh);
        var trangThaiDaDuyet = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.ThanhLyHopDong.DaDuyet);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy thanh lý hợp đồng");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        if (entity.TrangThaiId != trangThaiDaTrinh.Id)
        {
            throw new ManagedException("Chỉ có thể duyệt khi trạng thái là Đã trình");
        }

        entity.TrangThaiId = trangThaiDaDuyet.Id;

        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.ThanhLyHopDong,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaDuyet.Id,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}