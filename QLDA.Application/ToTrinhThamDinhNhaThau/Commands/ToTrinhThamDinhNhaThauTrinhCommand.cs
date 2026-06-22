using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Domain.Entities;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;

/// <summary>
/// Trình hồ sơ đề xuất cấp độ CNTT - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record ToTrinhThamDinhNhaThauTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class ToTrinhThamDinhNhaThauTrinhCommandHandler : IRequestHandler<ToTrinhThamDinhNhaThauTrinhCommand, int>
{
    private readonly IRepository<Domain.Entities.ToTrinhThamDinhNhaThau, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhThamDinhNhaThauTrinhCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.ToTrinhThamDinhNhaThau, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhThamDinhNhaThauTrinhCommand request, CancellationToken cancellationToken)
    {
        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy đề xuất.");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là Dự thảo");
        }

        entity.TrangThaiId = trangThaiDaTrinh.Id;
       // entity.NgayTrinh = DateTime.UtcNow;

        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.ToTrinhThamDinhNhaThau,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
