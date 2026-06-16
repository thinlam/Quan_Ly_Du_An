using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.ToTrinhKetQuaGoiThauMappings;
using QLDA.Application.ToTrinhKetQuaGoiThaus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.Commands;

public record ToTrinhKetQuaGoiThauInsertCommand(ToTrinhKetQuaGoiThauInsertDto Dto) : IRequest<ToTrinhKetQuaGoiThau>;

internal class ToTrinhKetQuaGoiThauInsertCommandHandler : IRequestHandler<ToTrinhKetQuaGoiThauInsertCommand, ToTrinhKetQuaGoiThau> {
    private readonly IRepository<ToTrinhKetQuaGoiThau, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhKetQuaGoiThauInsertCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhKetQuaGoiThau, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhKetQuaGoiThau> Handle(ToTrinhKetQuaGoiThauInsertCommand request,
        CancellationToken cancellationToken = default) {
        if (request.Dto.BuocId.HasValue) {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == request.Dto.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _user, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = new ToTrinhKetQuaGoiThau {
            So = request.Dto.So,
            BuocId = request.Dto.BuocId,
            DuAnId = request.Dto.DuAnId,
            NgayTrinh = request.Dto.NgayTrinh,
            TrichYeu = request.Dto.TrichYeu,
            TrangThaiDangTaiId = request.Dto.TrangThaiDangTaiId,
            TrangThaiId = trangThaiDuThao?.Id

        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        entity.SyncGoiThauIds(request.Dto.DanhSachGoiThau);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return entity;
    }
}
