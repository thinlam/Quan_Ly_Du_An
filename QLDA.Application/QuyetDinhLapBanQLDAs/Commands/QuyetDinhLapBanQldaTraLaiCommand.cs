using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Application.QuanLyPheDuyet.Commands;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.Commands;

/// <summary>
/// Trả lại phân khai kinh phí - LDDV role, cần lý do
/// </summary>
public record QuyetDinhLapBanQldaTraLaiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class QuyetDinhLapBanQldaTraLaiCommandHandler : IRequestHandler<QuyetDinhLapBanQldaTraLaiCommand, int> {
    private readonly DbContext _dbContext;
    private readonly IRepository<Domain.Entities.QuyetDinhLapBanQLDA, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepo;
    private readonly IDapperRepository _dapper;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;


    public QuyetDinhLapBanQldaTraLaiCommandHandler(DbContext dbContext, IServiceProvider serviceProvider) {
        _dbContext = dbContext;
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.QuyetDinhLapBanQLDA, Guid>>();
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

    public async Task<int> Handle(QuyetDinhLapBanQldaTraLaiCommand request, CancellationToken cancellationToken) {
        // Permission check: LDDV role only
        // Validate NoiDung is required
        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do trả lại là bắt buộc");
        }


        #region kiểm tra có tồn tại không, hiện đang có QuyetDinhLapBanQlda & QuyetDinhDuyetDuToan

        var entity = await _repository.GetQueryableSet()
                        .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định/tờ trình cần thao tác");

        if (entity == null)
            ManagedException.Throw("Không tìm thấy quyết định/tờ trình cần thao tác");

        #endregion
        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);
        #region kiểm tra có tồn tại quá trình thuyên chuyển của entity này
        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh);
        var trangThaiTra = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể trả lại khi trạng thái là Đã trình");
        }
        #endregion

        // Update status to Trả lại
        entity.TrangThaiId = trangThaiTra.Id;

        // Create history record with reason
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.QuyetDinhLapBanQLDA,
            EntityId = request.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTra.Id,
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
                PheDuyetEntityNames.QuyetDinhLapBanQLDA,
                entity.Id,
                entity.DuAn?.TenDuAn,
                entity.CreatedBy,
                PheDuyetNotificationAction.TraLai,
                request.NoiDung,
                cancellationToken: cancellationToken);
        }

        return affected;
    }
}