using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachLuaChonNhaThauRutGons.Commands;

/// <summary>
/// Trình hồ sơ đề xuất cấp độ CNTT - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record KeHoachLuaChonNhaThauRutGonTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class KeHoachLuaChonNhaThauRutGonTrinhCommandHandler : IRequestHandler<KeHoachLuaChonNhaThauRutGonTrinhCommand, int>
{
    private readonly IRepository<KeHoachLuaChonNhaThauRutGon, Guid> _repository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachLuaChonNhaThauRutGonTrinhCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThauRutGon, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(KeHoachLuaChonNhaThauRutGonTrinhCommand request, CancellationToken cancellationToken)
    {

        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.KeHoachLuaChonNhaThauRutGon.DaTrinh);
        var trangThaiDaChuyen = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.KeHoachLuaChonNhaThauRutGon.DaChuyen);

       // thực tế k dùng/ k có tờ trình -> ko vào quản lý phê duyệt, chỉ cần đổi trạng thái của entity

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái cần cập nhật!");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy đề xuất.");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDaChuyen?.Id)
        {
            throw new ManagedException("Trạng thái không thể trình");
        }

        entity.TrangThaiId = trangThaiDaTrinh.Id;


        // Create history record
        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh.Id,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
