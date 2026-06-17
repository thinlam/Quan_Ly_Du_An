using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Domain.Constants;
using System.Data;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;

namespace QLDA.Application.ChuTruongLapKeHoachs.Commands;

public record ChuTruongLapKeHoachInsertCommand(ChuTruongLapKeHoach Entity) : IRequest {
}

internal class
    ChuTruongLapKeHoachInsertCommandHandler : IRequestHandler<ChuTruongLapKeHoachInsertCommand> {
    private readonly IRepository<ChuTruongLapKeHoach, Guid> ChuTruongLapKeHoach;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> StatusRepo;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChuTruongLapKeHoachInsertCommandHandler> _logger;

    public ChuTruongLapKeHoachInsertCommandHandler(IServiceProvider serviceProvider,
        ILogger<ChuTruongLapKeHoachInsertCommandHandler> logger) {
        ChuTruongLapKeHoach = serviceProvider.GetRequiredService<IRepository<ChuTruongLapKeHoach, Guid>>();
        StatusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _logger = logger;
        _unitOfWork = ChuTruongLapKeHoach.UnitOfWork;
    }

    public async Task Handle(ChuTruongLapKeHoachInsertCommand request,
        CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");
            var trangThaiDuThao = await StatusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
            request.Entity.TrangThaiId = trangThaiDuThao?.Id;

            if (request.Entity.BuocId.HasValue) {
                var buoc = await _duAnBuocRepo.GetQueryableSet()
                    .Include(e => e.DuAn)
                    .Include(e => e.DuAnBuocPhongBanPhoiHops)
                    .FirstOrDefaultAsync(e => e.Id == request.Entity.BuocId.Value, cancellationToken);
                if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _authContext, cancellationToken))
                    throw new ManagedException("Phòng ban không có quyền thao tác bước này");
            }

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = ChuTruongLapKeHoach.GetQueryableSet().Any(o => o.Id == request.Entity.Id);
                if (isExist) {
                    await ChuTruongLapKeHoach.UpdateAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                } else {
                    await ChuTruongLapKeHoach.AddAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

   
}