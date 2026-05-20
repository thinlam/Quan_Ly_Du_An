using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.PhanKhaiKinhPhis.Commands;

/// <summary>
/// Từ chối phân khai kinh phí - tất cả roles quản lý, cần lý do
/// </summary>
public record PhanKhaiKinhPhiTuChoiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class PhanKhaiKinhPhiTuChoiCommandHandler : IRequestHandler<PhanKhaiKinhPhiTuChoiCommand, int> {
    private readonly IRepository<Domain.Entities.PhanKhaiKinhPhi, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IAppSettingsProvider _settings;
    private readonly IUnitOfWork _unitOfWork;

    public PhanKhaiKinhPhiTuChoiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.PhanKhaiKinhPhi, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(PhanKhaiKinhPhiTuChoiCommand request, CancellationToken cancellationToken) {
        // Permission: QLDA_LD, P.HC-TH (by PhongBanID), or QLDA_QuanTri
        var isHcth = _userProvider.Info.PhongBanID == _settings.PhongHCTHID;
        var isLanhDao = _userProvider.AuthInfo?.HasRole(QLDA.Domain.Constants.RoleConstants.QLDA_LDDV) ?? false;
        var isQuanTri = _userProvider.AuthInfo?.HasRole(QLDA.Domain.Constants.RoleConstants.QLDA_QuanTri) ?? false;
        if (!isLanhDao && !isHcth && !isQuanTri) {
            throw new ManagedException("Chỉ quản lý có quyền từ chối phân khai kinh phí");
        }

        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do từ chối là bắt buộc");
        }

        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DaTrinh && s.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi, cancellationToken);
        var trangThaiTuChoi = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.TraLai && s.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiTuChoi, "Không tìm thấy trạng thái 'Từ chối'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy phân khai kinh phí");

        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể từ chối khi trạng thái là Đã trình");
        }

        entity.TrangThaiId = trangThaiTuChoi.Id;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.PhanKhaiKinhPhi,
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