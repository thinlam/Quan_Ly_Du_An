using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.ToTrinhKetQuaGoiThauMappings;
using QLDA.Application.ToTrinhKetQuaGoiThaus.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.Commands;

public record ToTrinhKetQuaGoiThauInsertCommand(ToTrinhKetQuaGoiThauInsertDto Dto) : IRequest<ToTrinhKetQuaGoiThau>;

internal class ToTrinhKetQuaGoiThauInsertCommandHandler : IRequestHandler<ToTrinhKetQuaGoiThauInsertCommand, ToTrinhKetQuaGoiThau> {
    private readonly IRepository<ToTrinhKetQuaGoiThau, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhKetQuaGoiThauInsertCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhKetQuaGoiThau, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhKetQuaGoiThau> Handle(ToTrinhKetQuaGoiThauInsertCommand request,
        CancellationToken cancellationToken = default) {
        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var dto = request.Dto ?? new ToTrinhKetQuaGoiThauInsertDto();
        var entity = new ToTrinhKetQuaGoiThau {
            So = dto.So ?? string.Empty,
            BuocId = dto.BuocId,
            DuAnId = dto.DuAnId,
            NgayTrinh = dto.NgayTrinh,
            TrichYeu = dto.TrichYeu ?? string.Empty,
            TrangThaiDangTaiId = dto.TrangThaiDangTaiId,
            TrangThaiId = trangThaiDuThao?.Id

        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        entity.SyncGoiThauIds(dto.DanhSachGoiThau ?? new List<Guid>());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return entity!;
    }
}
