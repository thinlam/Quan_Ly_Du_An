using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Commands;

/// <summary>
/// Duyệt hồ sơ đề xuất cấp độ CNTT - chỉ BGĐ role
/// </summary>
public record HoSoDeXuatCapDoCnttDuyetCommand(Guid Id) : IRequest<int>;

internal class HoSoDeXuatCapDoCnttDuyetCommandHandler : IRequestHandler<HoSoDeXuatCapDoCnttDuyetCommand, int> {
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public HoSoDeXuatCapDoCnttDuyetCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(HoSoDeXuatCapDoCnttDuyetCommand request, CancellationToken cancellationToken) {
        if(_settings == null) {
            throw new ManagedException("Không lấy được cấu hình ứng dụng.");
        }
        var isHcth = _userProvider.Info.PhongBanID == _settings.PhongHCTHID;
        if (!_userProvider.AuthInfo.HasRole(Domain.Constants.RoleConstants.QLDA_LDDV) && !isHcth)
        {
            throw new ManagedException("Tài khoản không có quyền.");
        }

        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.DaTrinh && s.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt, cancellationToken);
        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.DaDuyet && s.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy hồ sơ đề xuất cấp độ CNTT");

        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể duyệt khi trạng thái là Đã trình");
        }

        entity.TrangThaiId = trangThaiDaDuyet.Id;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.HoSoDeXuatCapDoCntt,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaDuyet.Id,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
