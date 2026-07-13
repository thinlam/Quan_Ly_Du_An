using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Application.ThongBaos.Commands;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.DeXuatChuyenTieps.Commands;

/// <summary>
/// Duyệt phân khai kinh phí - LDDV role
/// </summary>
public record DeXuatChuyenTiepDuyetCommand(Guid Id) : IRequest<int>;

internal class DeXuatChuyenTiepDuyetCommandHandler : IRequestHandler<DeXuatChuyenTiepDuyetCommand, int> {
    private readonly IRepository<Domain.Entities.DeXuatChuyenTiep, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<PheDuyet, Guid> _PheDuyetRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;
    private readonly IMediator _mediator;
    private readonly ILogger<DeXuatChuyenTiepDuyetCommandHandler> _logger;

    public DeXuatChuyenTiepDuyetCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.DeXuatChuyenTiep, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _PheDuyetRepository = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
        _logger = serviceProvider.GetRequiredService<ILogger<DeXuatChuyenTiepDuyetCommandHandler>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatChuyenTiepDuyetCommand request, CancellationToken cancellationToken) {
        // Get status IDs from DB by code
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        var entity = await _repository.GetQueryableSet().Include(x => x.DuAn).ThenInclude(x => x.BuocHienTai)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");

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
            EntityName = PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaDuyet.Id,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (result <= 0) {
            return result;
        }

        // thông báo tới người dùng (sau khi save thành công)
        try {
            var body = $"Tờ trình/phê duyệt <b>{PheDuyetEntityNames.DeXuatChuTruongChuyenTiep.GetDescriptionFromName()}" +
                        $"</b> giá trị giải ngân <b>{entity.SoLieuGiaiNgan}</b> của dự án <b>{entity.DuAn?.TenDuAn}</b> - " +
                        $"<b>{entity.DuAn?.BuocHienTai?.TenBuoc}</b> đã được duyệt";
            var nguoiGuiId = _userProvider.Info.UserID;
            var pheDuyet = await _PheDuyetRepository.GetQueryableSet()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(
                                e => e.EntityId == request.Id
                                     && e.EntityName == PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
                                cancellationToken);
            if (pheDuyet != null) {
                var nguoiNhanId = pheDuyet.NguoiTrinhId ?? 0;
                if (nguoiNhanId > 0 && nguoiGuiId > 0) {
                    await _mediator.Send(
                        new ThongBaoInsertCommand(nguoiGuiId, nguoiNhanId, body),
                        cancellationToken);
                }
            }
        } catch (Exception ex) {
            _logger.LogError(
                ex,
                "Không thể gửi thông báo sau khi phê duyệt đề xuất chuyển tiếp {EntityId}",
                request.Id);
        }

        return result;
    }
}
