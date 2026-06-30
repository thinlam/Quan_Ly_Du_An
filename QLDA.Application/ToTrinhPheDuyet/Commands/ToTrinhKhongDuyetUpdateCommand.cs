using BuildingBlocks.CrossCutting.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Application.ToTrinhPheDuyets.DTOs;
using QLDA.Domain.Constants;
using System.Data;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

public record ToTrinhKhongDuyetUpdateCommand(ToTrinhPheDuyetInsUpdDto Dto) : IRequest<ToTrinhPheDuyet>;

internal class ToTrinhKhongDuyetUpdateCommandHandler : IRequestHandler<ToTrinhKhongDuyetUpdateCommand, ToTrinhPheDuyet>
{
    private readonly IRepository<ToTrinhPheDuyet, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IAppSettingsProvider _settings;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhKhongDuyetUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhPheDuyet> Handle(ToTrinhKhongDuyetUpdateCommand request, CancellationToken cancellationToken = default)
    {
        var statuses = await _statusRepo.GetByLoaiAsync(PheDuyetEntityNames.ToTrinhKhongDuyet, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatKhongDuyet.DuThao);
        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatKhongDuyet.DaTrinh);
        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        // Phòng KHTC (HasKhtcBypass) được phép cập nhật mọi trạng thái; các role khác chỉ cập nhật được khi trạng thái = Dự thảo.
        if (!_authContext.HasKhtcBypass && entity.TrangThaiId != trangThaiDuThao?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật!");
        }

        entity.So = request.Dto.So;
        entity.Ten = request.Dto.Ten;
        entity.NgayToTrinh = request.Dto.NgayToTrinh;
        entity.TrichYeu = request.Dto.TrichYeu;
        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}

