using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;

namespace QLDA.Application.ChuTruongLapKeHoachs.Commands;

/// <summary>
/// Duyệt phân khai kinh phí - LDDV role
/// </summary>
public record ChuTruongLapKeHoachDuyetCommand(Guid Id, string? NoiDung) : IRequest<int>;

internal class ChuTruongLapKeHoachDuyetCommandHandler : IRequestHandler<ChuTruongLapKeHoachDuyetCommand, int> {
    private readonly IRepository<Domain.Entities.ChuTruongLapKeHoach, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public ChuTruongLapKeHoachDuyetCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.ChuTruongLapKeHoach, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ChuTruongLapKeHoachDuyetCommand request, CancellationToken cancellationToken) {

        // Get status IDs from DB by code
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh
            && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh!.Id) {
            throw new ManagedException("Chỉ có thể duyệt khi trạng thái là Đã trình");
        }

        // Update status to Đã duyệt
        entity.TrangThaiId = trangThaiDaDuyet!.Id;
        entity.ButPhe = request.NoiDung;
        var ngayToTrinh = entity.NgayToTrinh.ToDateOnlyVn();
        // Create history record
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.ChuTruongLapKeHoach,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NoiDung = !string.IsNullOrEmpty(request.NoiDung) ? request.NoiDung
                        : $"{entity.SoToTrinh} - {(ngayToTrinh.HasValue ? ngayToTrinh.Value.ToString("dd/MM/yyyy") : "")}",
            EntityId = entity.Id,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaDuyet!.Id,
            NgayXuLy = DateTimeOffset.UtcNow
        };
       
        await _repository.UpdateAsync(entity, cancellationToken);
        await _historyRepository.AddAsync(history, cancellationToken);// has trigger của table PheDuyetHistory

        return await _unitOfWork.SaveChangesAsync(cancellationToken);

    }
}
