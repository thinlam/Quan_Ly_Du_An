using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Commands;

/// <summary>
/// Trình hồ sơ đề xuất cấp độ CNTT - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record HoSoDeXuatCapDoCnttTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class HoSoDeXuatCapDoCnttTrinhCommandHandler : IRequestHandler<HoSoDeXuatCapDoCnttTrinhCommand, int> {
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoDeXuatCapDoCnttTrinhCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(HoSoDeXuatCapDoCnttTrinhCommand request, CancellationToken cancellationToken) {
        var phongBanId = _userProvider.Info.PhongBanID;
        if (phongBanId != 219) {
            throw new ManagedException("Chỉ phòng KH-TC có quyền trình hồ sơ đề xuất cấp độ CNTT");
        }

        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.DuThao && s.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.TraLai && s.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt, cancellationToken);
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.DaTrinh && s.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy hồ sơ đề xuất cấp độ CNTT");

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id) {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là Dự thảo");
        }

        entity.TrangThaiId = trangThaiDaTrinh.Id;
        entity.NgayTrinh = DateTime.UtcNow;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.HoSoDeXuatCapDoCntt,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
