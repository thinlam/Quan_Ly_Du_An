using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.PhanKhaiKinhPhis.Commands;

/// <summary>
/// Trình phân khai kinh phí - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record PhanKhaiKinhPhiTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class PhanKhaiKinhPhiTrinhCommandHandler : IRequestHandler<PhanKhaiKinhPhiTrinhCommand, int> {
    private readonly IRepository<Domain.Entities.PhanKhaiKinhPhi, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public PhanKhaiKinhPhiTrinhCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.PhanKhaiKinhPhi, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(PhanKhaiKinhPhiTrinhCommand request, CancellationToken cancellationToken) {
        // Permission check: KH-TC only (PhongBanId = 219)
        //var phongBanId = _userProvider.Info.PhongBanID;
        //if (phongBanId != 219) {
        //    throw new ManagedException("Chỉ phòng KH-TC có quyền trình phân khai kinh phí");
        //}

        // Get status IDs from DB by code
        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DuThao && s.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.TraLai && s.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi, cancellationToken);
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DaTrinh && s.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy phân khai kinh phí");

        // Validate current status must be null (legacy), Dự thảo, or Trả lại
        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id) {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là Dự thảo");
        }

        // Update status to Đã trình
        entity.TrangThaiId = trangThaiDaTrinh.Id;

        // Create history record
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.PhanKhaiKinhPhi,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}