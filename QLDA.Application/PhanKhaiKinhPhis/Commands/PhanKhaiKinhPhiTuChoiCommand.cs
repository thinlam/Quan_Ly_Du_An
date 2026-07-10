using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Application.QuanLyPheDuyet.Commands;

namespace QLDA.Application.PhanKhaiKinhPhis.Commands;

/// <summary>
/// Từ chối phân khai kinh phí - tất cả roles quản lý, cần lý do
/// </summary>
public record PhanKhaiKinhPhiTuChoiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class PhanKhaiKinhPhiTuChoiCommandHandler : IRequestHandler<PhanKhaiKinhPhiTuChoiCommand, int> {
    private readonly IRepository<Domain.Entities.PhanKhaiKinhPhi, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepo;
    private readonly IDapperRepository _dapper;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IAppSettingsProvider _settings;
    private readonly IUnitOfWork _unitOfWork;

    public PhanKhaiKinhPhiTuChoiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.PhanKhaiKinhPhi, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _pheDuyetRepo = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(PhanKhaiKinhPhiTuChoiCommand request, CancellationToken cancellationToken) {
        // Permission: QLDA_LD or QLDA_QuanTri

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
            .Include(e => e.DuAn)
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
                PheDuyetNotificationAction.TuChoi,
                request.NoiDung,
                maTrangThaiTrinh: TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DaTrinh,
                cancellationToken: cancellationToken);
        }

        return affected;
    }
}