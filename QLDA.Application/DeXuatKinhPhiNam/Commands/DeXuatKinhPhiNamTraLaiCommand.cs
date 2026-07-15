using Microsoft.EntityFrameworkCore;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.Commands;

/// <summary>
/// Trả lại phân khai kinh phí - LDDV role, cần lý do
/// </summary>
public record DeXuatNhuCauKinhPhiNamTraLaiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class DeXuatNhuCauKinhPhiNamTraLaiCommandHandler : IRequestHandler<DeXuatNhuCauKinhPhiNamTraLaiCommand, int> {
    private readonly IRepository<Domain.Entities.DeXuatNhuCauKinhPhiNam, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public DeXuatNhuCauKinhPhiNamTraLaiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.DeXuatNhuCauKinhPhiNam, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatNhuCauKinhPhiNamTraLaiCommand request, CancellationToken cancellationToken) {
        // Permission check: LDDV role only

        // Validate NoiDung is required
        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do trả lại là bắt buộc");
        }

        // Get status IDs from DB by code
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.DaTrinh && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.TraLai && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");
        ManagedException.ThrowIfNull(trangThaiTraLai, "Không tìm thấy trạng thái 'Trả lại'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");

        // Validate current status must be Đã trình
        if (entity.TrangThaiId != trangThaiDaTrinh!.Id) {
            throw new ManagedException("Chỉ có thể trả lại khi trạng thái là Đã trình");
        }

        // Update status to Trả lại
        entity.TrangThaiId = trangThaiTraLai!.Id;

        // Create history record with reason
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam,
            EntityId = entity.Id,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTraLai!.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}