using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

/// <summary>
/// Trình hồ sơ mời thầu điện tử - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record HoSoMoiThauDienTuTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class HoSoMoiThauDienTuTrinhCommandHandler : IRequestHandler<HoSoMoiThauDienTuTrinhCommand, int> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuTrinhCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(HoSoMoiThauDienTuTrinhCommand request, CancellationToken cancellationToken) {
        var phongBanId = _userProvider.Info.PhongBanID;
        if (phongBanId != 219) {
            throw new ManagedException("Chỉ phòng KH-TC có quyền trình hồ sơ mời thầu điện tử");
        }

        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DuThao && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.TraLai && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaTrinh && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy hồ sơ mời thầu điện tử");

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id) {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là Dự thảo");
        }

        entity.TrangThaiId = trangThaiDaTrinh.Id;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.HoSoMoiThauDienTu,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId ?? Guid.Empty,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
