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
/// Trả lại phân khai kinh phí - LDDV role, cần lý do
/// </summary>
public record DeXuatChuyenTiepTraLaiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class DeXuatChuyenTiepTraLaiCommandHandler : IRequestHandler<DeXuatChuyenTiepTraLaiCommand, int> {
    private readonly IRepository<Domain.Entities.DeXuatChuyenTiep, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;
    private readonly IMediator _mediator;
    private readonly ILogger<DeXuatChuyenTiepTraLaiCommandHandler> _logger;

    public DeXuatChuyenTiepTraLaiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.DeXuatChuyenTiep, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _pheDuyetRepository = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
        _logger = serviceProvider.GetRequiredService<ILogger<DeXuatChuyenTiepTraLaiCommandHandler>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatChuyenTiepTraLaiCommand request, CancellationToken cancellationToken) {

        // Validate NoiDung is required
        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do trả lại là bắt buộc");
        }

        // Get status IDs from DB by code
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiTraLai, "Không tìm thấy trạng thái 'Trả lại'");

        var entity = await _repository.GetQueryableSet()
            .Include(x => x.DuAn)
            .ThenInclude(x => x.BuocHienTai)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh.Id) {
            throw new ManagedException("Chỉ có thể trả lại khi trạng thái là Đã trình");
        }

        // Update status to Trả lại
        entity.TrangThaiId = trangThaiTraLai.Id;

        // Create history record with reason
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTraLai.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (result <= 0) {
            return result;
        }

        // thông báo tới người trình (sau khi save thành công)
        try {
            var body = $"Tờ trình/phê duyệt <b>{PheDuyetEntityNames.DeXuatChuTruongChuyenTiep.GetDescriptionFromName()}" +
                        $"</b> giá trị giải ngân <b>{entity.SoLieuGiaiNgan}</b> của dự án <b>{entity.DuAn?.TenDuAn}</b> - " +
                        $"<b>{entity.DuAn?.BuocHienTai?.TenBuoc}</b> đã bị trả lại. Lý do: {request.NoiDung}";
            var nguoiGuiId = _userProvider.Info.UserID;
            var pheDuyet = await _pheDuyetRepository.GetQueryableSet()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(
                                e => e.EntityId == request.Id
                                     && e.EntityName == PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
                                cancellationToken);

            long nguoiNhanId = pheDuyet?.NguoiTrinhId ?? 0;

            // Fallback: NguoiTrinhId thường null — lấy người trình từ history ĐTr
            if (nguoiNhanId <= 0) {
                var nguoiTrinhTuHistory = await _historyRepository.GetQueryableSet()
                    .AsNoTracking()
                    .Where(h => h.EntityId == request.Id
                                && h.EntityName == PheDuyetEntityNames.DeXuatChuTruongChuyenTiep
                                && h.TrangThaiId == trangThaiDaTrinh.Id
                                && h.NguoiXuLyId != null
                                && h.NguoiXuLyId > 0)
                    .OrderByDescending(h => h.NgayXuLy)
                    .Select(h => h.NguoiXuLyId)
                    .FirstOrDefaultAsync(cancellationToken);
                nguoiNhanId = nguoiTrinhTuHistory ?? 0;
            }

            if (nguoiNhanId > 0 && nguoiGuiId > 0) {
                await _mediator.Send(
                    new ThongBaoInsertCommand(nguoiGuiId, nguoiNhanId, body),
                    cancellationToken);
            } else {
                _logger.LogWarning(
                    "Không gửi thông báo trả lại vì thiếu NguoiNhanId/NguoiGuiId. EntityId: {EntityId}",
                    request.Id);
            }
        } catch (Exception ex) {
            _logger.LogError(
                ex,
                "Không thể gửi thông báo sau khi trả lại đề xuất chuyển tiếp {EntityId}",
                request.Id);
        }

        return result;
    }
}
