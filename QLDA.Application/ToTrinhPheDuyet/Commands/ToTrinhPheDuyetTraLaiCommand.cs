using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

/// <summary>
/// Mục đích phục vụ cho các loại tờ trình  cần duyệt( chỉ trình )

/// Mục đích phục vụ cho các loại tờ trình không cần duyệt( chỉ trình )
//  Table ToTrinhPheDuyet có 2 loại
//1. Đầy đủ các bước trình/duyệt/trả
/// </summary>
public record ToTrinhPheDuyetTraLaiCommand(Guid Id,string Loai, string NoiDung) : IRequest<int>;

internal class ToTrinhPheDuyetTraLaiCommandHandler : IRequestHandler<ToTrinhPheDuyetTraLaiCommand, int> {
    private readonly DbContext _dbContext;
    private readonly IRepository<Domain.Entities.ToTrinhPheDuyet, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;


    public ToTrinhPheDuyetTraLaiCommandHandler(DbContext dbContext, IServiceProvider serviceProvider) {
        _dbContext = dbContext;
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.ToTrinhPheDuyet, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();

        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhPheDuyetTraLaiCommand request, CancellationToken cancellationToken) {
        // Permission check: LDDV role only
        // Validate NoiDung is required
        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do trả lại là bắt buộc");
        }


        //ToTrinhPheDuyet : QuyetDinhDuyetDuToan, ToTrinhKeHoach, QuyetDinhKeHoachThue, PheDuyetKhaoSat
        #region kiểm tra có tồn tại không, hiện đang có ToTrinhPheDuyet & QuyetDinhDuyetDuToan

        string table = request.Loai;
        if (ToTrinhEntityNamesExtensions.ContainsEntity(request.Loai))
            table = "ToTrinhPheDuyet";

        var entityType = _dbContext.Model.GetEntityTypes()
                .FirstOrDefault(t => t.ClrType.Name == table)?.ClrType;
        ManagedException.ThrowIfNull(entityType, "Không tìm thấy entity type");

        var entity = await _dbContext.FindAsync(entityType, new object[] { request.Id }, cancellationToken) as IApprovableEntity;

        if (entity == null)
            ManagedException.Throw("Không tìm thấy quyết định/tờ trình cần thao tác");
        var entitySafe = entity!;

        await _auth.EnsureCanExecuteStepAsync(entitySafe.BuocId, _authContext, cancellationToken);

        #endregion
        #region 
        bool isKhongDuyet = LoaiToTrinhKhongDuyetExtensions.ContainsDescription(request.Loai);
        var loaiPheDuyet = isKhongDuyet ? PheDuyetEntityNames.ToTrinhKhongDuyet : PheDuyetEntityNames.DeXuatMacDinhStt;
        var statuses = await _statusRepository.GetByLoaiAsync(loaiPheDuyet, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh);
        var trangThaiTra = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        // Validate current status must be Đã trình
        if (entitySafe.TrangThaiId != trangThaiDaTrinh!.Id) {
            throw new ManagedException("Chỉ có thể trả lại khi trạng thái là Đã trình");
        }
        #endregion

        // Update status to Trả lại
        entitySafe.TrangThaiId = trangThaiTra!.Id;

        // Create history record with reason
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = request.Loai,
            EntityId = request.Id,
            DuAnId = entitySafe.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTra!.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
