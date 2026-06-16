using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;

namespace QLDA.Application.DangTaiKeHoachLcntLenMangs.Commands;

public record DangTaiKeHoachLcntLenMangInsertOrUpdateCommand(DangTaiKeHoachLcntLenMang Entity) : IRequest {
}

internal class DangTaiKeHoachLcntLenMangInsertOrUpdateCommandHandler : IRequestHandler<DangTaiKeHoachLcntLenMangInsertOrUpdateCommand> {
    private readonly IRepository<DangTaiKeHoachLcntLenMang, Guid> DangTaiKeHoachLcntLenMang;
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau;
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DangTaiKeHoachLcntLenMangInsertOrUpdateCommandHandler> _logger;

    public DangTaiKeHoachLcntLenMangInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<DangTaiKeHoachLcntLenMangInsertOrUpdateCommandHandler> logger) {
        DangTaiKeHoachLcntLenMang = serviceProvider.GetRequiredService<IRepository<DangTaiKeHoachLcntLenMang, Guid>>();
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        KeHoachLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _logger = logger;
        _unitOfWork = DangTaiKeHoachLcntLenMang.UnitOfWork;
    }

    public async Task Handle(DangTaiKeHoachLcntLenMangInsertOrUpdateCommand request, CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");

            ManagedException.ThrowIf(!KeHoachLuaChonNhaThau.GetOrderedSet().Any(e => e.Id == request.Entity.KeHoachLuaChonNhaThauId),
                "Không tồn tại kế hoạch lcnt");

            if (request.Entity.BuocId.HasValue)
            {
                var buoc = await _duAnBuocRepo.GetQueryableSet()
                    .Include(e => e.DuAn)
                    .Include(e => e.DuAnBuocPhongBanPhoiHops)
                    .FirstOrDefaultAsync(e => e.Id == request.Entity.BuocId.Value, cancellationToken);
                if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _userProvider, cancellationToken))
                    throw new ManagedException("Phòng ban không có quyền thao tác bước này");
            }

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = DangTaiKeHoachLcntLenMang.GetQueryableSet().Any(o => o.Id == request.Entity.Id);
                if (isExist) {
                    await DangTaiKeHoachLcntLenMang.UpdateAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                } else {
                    ManagedException.ThrowIf(DangTaiKeHoachLcntLenMang.GetOrderedSet().Any(e=>e.KeHoachLuaChonNhaThauId == request.Entity.KeHoachLuaChonNhaThauId),"Mỗi kế hoạch lcnt chỉ được đăng 1 lần");
                    await DangTaiKeHoachLcntLenMang.AddAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                //Cập nhật quy trình
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
