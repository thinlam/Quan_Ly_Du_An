using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.Commands;

/// <summary>
/// Trình quyết định điều chỉnh - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record QuyetDinhLapBanQldaTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class QuyetDinhLapBanQldaTrinhCommandHandler : IRequestHandler<QuyetDinhLapBanQldaTrinhCommand, int>
{
    private readonly DbContext _dbContext;
    private readonly IRepository<QuyetDinhLapBanQLDA, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public QuyetDinhLapBanQldaTrinhCommandHandler(DbContext dbContext, IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhLapBanQLDA, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhLapBanQldaTrinhCommand request, CancellationToken cancellationToken)
    {
       
        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);
        var trangThaiTraLai = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai);
        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");


        var entity = await _repository.GetQueryableSet()
       .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định/tờ trình cần thao tác");
        var entitySafe = entity!;

        // Validate: must be DT (Dự thảo) or TL (Trả lại) to transition to ĐTr (Đã trình)
        if (entitySafe.TrangThaiId != trangThaiDuThao?.Id && entitySafe.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Trạng thái không thể trình!");
        }
        //Kiểm tra quyền  thao tác
        await _auth.EnsureCanExecuteStepAsync(entitySafe.BuocId, _authContext, cancellationToken);

        // 5. Cập nhật TrangThaiId (Dù là Model nào cũng chỉ tốn đúng 1 dòng này)
        entitySafe.TrangThaiId = trangThaiDaTrinh!.Id;

        // 6. Lưu lịch sử phê duyệt
        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.QuyetDinhLapBanQLDA,
            EntityId = request.Id,
            DuAnId = entitySafe.DuAnId,
            BuocId = entitySafe.BuocId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh!.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };
        await _historyRepository.AddAsync(history);
       
        await _dbContext.SaveChangesAsync(cancellationToken);
        return 1;


    }



}
