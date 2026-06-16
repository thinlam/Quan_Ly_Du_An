using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ThoaThuanGiaoViecs.Commands;

/// <summary>
/// Trình hồ sơ đề xuất cấp độ CNTT - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record ThoaThuanGiaoViecChuyenCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class ThoaThuanGiaoViecChuyenCommandHandler : IRequestHandler<ThoaThuanGiaoViecChuyenCommand, int>
{
    private readonly IRepository<ThoaThuanGiaoViec, Guid> _repository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ThoaThuanGiaoViecChuyenCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<ThoaThuanGiaoViec, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ThoaThuanGiaoViecChuyenCommand request, CancellationToken cancellationToken)
    {
        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.ThoaThuanGiaoViec.DuThao);
        var trangThaiDaChuyen = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.ThoaThuanGiaoViec.DaChuyen);
        ManagedException.ThrowIfNull(trangThaiDaChuyen, "Không tìm thấy trạng thái 'Đã chuyển'");

        var entity = await _repository.GetQueryableSet().FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy đề xuất.");

        if (entity.BuocId.HasValue) {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == entity.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _userProvider, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id)
        {
            throw new ManagedException("Trạng thái không thể chuyển");
        }

        entity.TrangThaiId = trangThaiDaChuyen.Id;
        // entity.NgayTrinh = DateTime.UtcNow;
        // Create history record
        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.ThoaThuanGiaoViec,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaChuyen.Id,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
