using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

/// <summary>
/// Duyệt phân khai kinh phí - LDDV role
/// </summary>
public record ToTrinhPheDuyetDuyetCommand(Guid Id, string Loai) : IRequest<int>;

internal class ToTrinhPheDuyetDuyetCommandHandler : IRequestHandler<ToTrinhPheDuyetDuyetCommand, int> {
    private readonly DbContext _dbContext;
    private readonly IRepository<Domain.Entities.ToTrinhPheDuyet, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public ToTrinhPheDuyetDuyetCommandHandler(DbContext dbContext, IServiceProvider serviceProvider) {
        _dbContext = dbContext;
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.ToTrinhPheDuyet, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhPheDuyetDuyetCommand request, CancellationToken cancellationToken) {
        var isHcth = _userProvider.Info.PhongBanID == _settings.PhongHCTHId;
        if (!_userProvider.AuthInfo.HasRole(Domain.Constants.RoleConstants.QLDA_LDDV) && !isHcth)
        {
            throw new ManagedException("Tài khoản không có quyền.");
        }

        bool isKhongDuyet = LoaiToTrinhKhongDuyetExtensions.ContainsDescription(request.Loai);

        var loaiPheDuyet = isKhongDuyet ? PheDuyetEntityNames.ToTrinhKhongDuyet : PheDuyetEntityNames.DeXuatMacDinhStt;
        var statuses = await _statusRepository.GetByLoaiAsync(loaiPheDuyet, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh);
        var trangThaiDaDuyet = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        string table = request.Loai;
        if (ToTrinhEntityNamesExtensions.ContainsEntity(request.Loai))
            table = "ToTrinhPheDuyet";// hiện đang có ToTrinhPheDuyet & QuyetDinhDuyetDuToan

        var entityType = _dbContext.Model.GetEntityTypes()
                .FirstOrDefault(t => t.ClrType.Name == table)?.ClrType;

        var entity = await _dbContext.FindAsync(entityType, new object[] { request.Id }, cancellationToken) as IApprovableEntity;
        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu cần cập nhật");


        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể duyệt khi trạng thái là Đã trình");
        }

        // Update status to Đã duyệt
        entity.TrangThaiId = trangThaiDaDuyet.Id;

        // Create history record
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = request.Loai,// get Loai from request.Loai if needed
            EntityId = request.Id,
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