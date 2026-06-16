using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Entities;

namespace QLDA.Application.DeXuatChuyenTieps.Commands;

public record DeXuatChuyenTiepDeleteCommand(Guid Id) : IRequest<int>
{
}

public record DeXuatChuyenTiepDeleteCommandHandler : IRequestHandler<DeXuatChuyenTiepDeleteCommand, int>
{
    private readonly IRepository<DeXuatChuyenTiep, Guid> DeXuatChuyenTiep;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatChuyenTiepDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        DeXuatChuyenTiep = serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = DeXuatChuyenTiep.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatChuyenTiepDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await DeXuatChuyenTiep.GetOrderedSet()
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