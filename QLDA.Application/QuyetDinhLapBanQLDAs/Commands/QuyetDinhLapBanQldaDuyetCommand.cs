using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using BuildingBlocks.Domain.Providers;
using global::QLDA.Application.Authorization;
using global::QLDA.Application.Providers;
using global::QLDA.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.QuanLyPheDuyet.Commands;


namespace QLDA.Application.QuyetDinhLapBanQLDAs.Commands;


public record QuyetDinhLapBanQldaDuyetCommand(Guid Id) : IRequest<int>;

internal class QuyetDinhLapBanQldaDuyetCommandHandler : IRequestHandler<QuyetDinhLapBanQldaDuyetCommand, int>
{
    private readonly DbContext _dbContext;
    private readonly IRepository<QuyetDinhLapBanQLDA, Guid> _repository; 
    private readonly IRepository<VanBanQuyetDinh, Guid> _vanBanQuyetDinh; 
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepo;
    private readonly IDapperRepository _dapper;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public QuyetDinhLapBanQldaDuyetCommandHandler(DbContext dbContext, IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _vanBanQuyetDinh = serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhLapBanQLDA, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _pheDuyetRepo = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhLapBanQldaDuyetCommand request, CancellationToken cancellationToken)
    {

        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh);
        var trangThaiDaDuyet = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

      
        var entity = await _repository.GetQueryableSet()
        .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu cần cập nhật");


        //await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh.Id)
        {
            throw new ManagedException("Chỉ có thể duyệt khi trạng thái là Đã trình");
        }

        // Update status to Đã duyệt
        entity.TrangThaiId = trangThaiDaDuyet.Id;

        // Create history record
        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.QuyetDinhLapBanQLDA,// get Loai from request.Loai if needed
            EntityId = request.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaDuyet.Id,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);
        //save to VanBanQuyetDinh
        var vb = new VanBanQuyetDinh
        {
            Id = Guid.NewGuid(),
            So = entity.So,
            TrichYeu = entity.TrichYeu,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            Loai = PheDuyetEntityNames.QuyetDinhLapBanQLDA,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy
        };

        await _vanBanQuyetDinh.AddAsync(vb, cancellationToken);

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
                PheDuyetNotificationAction.Duyet,
                cancellationToken: cancellationToken);
        }

        return affected;
    }
}