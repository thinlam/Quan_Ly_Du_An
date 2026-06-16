using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Entities;

namespace QLDA.Application.ChuTruongLapKeHoachs.Commands;

public record ChuTruongLapKeHoachDeleteCommand(Guid Id) : IRequest<int> {
}

public record ChuTruongLapKeHoachDeleteCommandHandler : IRequestHandler<ChuTruongLapKeHoachDeleteCommand, int> {
    private readonly IRepository<ChuTruongLapKeHoach, Guid> ChuTruongLapKeHoach;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public ChuTruongLapKeHoachDeleteCommandHandler(IServiceProvider serviceProvider) {
        ChuTruongLapKeHoach = serviceProvider.GetRequiredService<IRepository<ChuTruongLapKeHoach, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = ChuTruongLapKeHoach.UnitOfWork;
    }

    public async Task<int> Handle(ChuTruongLapKeHoachDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await ChuTruongLapKeHoach.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        if (entity.BuocId.HasValue) {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == entity.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _user, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }
        
        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}