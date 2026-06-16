using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ThoaThuanGiaoViecs.Commands;

/// <summary>
/// Trình hồ sơ đề xuất cấp độ CNTT - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record ThoaThuanGiaoViecTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class ThoaThuanGiaoViecTrinhCommandHandler : IRequestHandler<ThoaThuanGiaoViecTrinhCommand, int>
{
    private readonly IRepository<ThoaThuanGiaoViec, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;

    public ThoaThuanGiaoViecTrinhCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<ThoaThuanGiaoViec, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ThoaThuanGiaoViecTrinhCommand request, CancellationToken cancellationToken)
    {
       // chỉ user Phòng KHTC mới dc trình
       var isHcth = _userProvider.Info.PhongBanID == _settings.PhongHCTHID;
        if (!isHcth)
            throw new ManagedException("Tài khoản không có quyền.");

        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.ThoaThuanGiaoViec, cancellationToken);
        var statusDict = statuses.ToDictionary(x => x.Ma);

        var trangThaiDaChuyen = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.ThoaThuanGiaoViec.DaChuyen);
        var trangThaiTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.ThoaThuanGiaoViec.DaTrinh);
        ManagedException.ThrowIfNull(trangThaiTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy đề xuất.");

        if (entity.BuocId.HasValue) {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == entity.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _userProvider, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDaChuyen?.Id)
        {
            throw new ManagedException("Trạng thái không thể trình!");
        }

        entity.TrangThaiId = trangThaiTrinh.Id;
       // entity.NgayTrinh = DateTime.UtcNow;

        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.ThoaThuanGiaoViec,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTrinh.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
