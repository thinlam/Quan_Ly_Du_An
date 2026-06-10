using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachLuaChonNhaThauRutGons.Commands;

/// <summary>
/// Trình hồ sơ đề xuất cấp độ CNTT - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record KeHoachLuaChonNhaThauRutGonChuyenCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class KeHoachLuaChonNhaThauRutGonChuyenCommandHandler : IRequestHandler<KeHoachLuaChonNhaThauRutGonChuyenCommand, int>
{
    private readonly IRepository<KeHoachLuaChonNhaThauRutGon, Guid> _repository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachLuaChonNhaThauRutGonChuyenCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThauRutGon, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(KeHoachLuaChonNhaThauRutGonChuyenCommand request, CancellationToken cancellationToken)
    {
        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.KeHoachLuaChonNhaThauRutGon.DuThao && s.Loai == PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);
        var trangThaiDaChuyen = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.KeHoachLuaChonNhaThauRutGon.DaChuyen && s.Loai == PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);
        // thực tế k dùng/ k có tờ trình -> ko vào quản lý phê duyệt, chỉ cần đổi trạng thái của entity

        ManagedException.ThrowIfNull(trangThaiDaChuyen, "Không tìm thấy trạng thái 'Đã chuyển'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy đề xuất.");

        if (entity.TrangThaiId != null &&  entity.TrangThaiId != trangThaiDuThao?.Id)
        {
            throw new ManagedException("Trạng thái không thể chuyển");
        }

        entity.TrangThaiId = trangThaiDaChuyen.Id;
        // entity.NgayTrinh = DateTime.UtcNow;
        // Create history record
        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon,
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
