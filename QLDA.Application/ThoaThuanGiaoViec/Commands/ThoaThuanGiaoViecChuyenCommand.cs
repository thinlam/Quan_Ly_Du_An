using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.ThoaThuanGiaoViecs.Commands;

/// <summary>
///
/// </summary>
public record ThoaThuanGiaoViecChuyenCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class ThoaThuanGiaoViecChuyenCommandHandler : IRequestHandler<ThoaThuanGiaoViecChuyenCommand, int>
{
    private readonly IRepository<ThoaThuanGiaoViec, Guid> _repository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ThoaThuanGiaoViecChuyenCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<ThoaThuanGiaoViec, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ThoaThuanGiaoViecChuyenCommand request, CancellationToken cancellationToken)
    {
        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.ThoaThuanGiaoViec, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.TrangThaiPhongKHTCPhuTrach.DuThao);
        var trangThaiDaChuyen = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.TrangThaiPhongKHTCPhuTrach.DaChuyen);
        ManagedException.ThrowIfNull(trangThaiDaChuyen, "Không tìm thấy trạng thái 'Đã chuyển'");

        var entity = await _repository.GetQueryableSet().FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy đề xuất.");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

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
