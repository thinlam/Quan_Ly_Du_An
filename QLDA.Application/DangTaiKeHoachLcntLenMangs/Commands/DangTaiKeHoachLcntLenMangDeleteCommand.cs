using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Entities;

namespace QLDA.Application.DangTaiKeHoachLcntLenMangs.Commands;

public record DangTaiKeHoachLcntLenMangDeleteCommand(Guid Id) : IRequest<int> {
}

public record DangTaiKeHoachLcntLenMangDeleteCommandHandler : IRequestHandler<DangTaiKeHoachLcntLenMangDeleteCommand, int> {
    private readonly IRepository<DangTaiKeHoachLcntLenMang, Guid> DangTaiKeHoachLcntLenMang;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public DangTaiKeHoachLcntLenMangDeleteCommandHandler(IServiceProvider serviceProvider) {
        DangTaiKeHoachLcntLenMang = serviceProvider.GetRequiredService<IRepository<DangTaiKeHoachLcntLenMang, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = DangTaiKeHoachLcntLenMang.UnitOfWork;
    }

    public async Task<int> Handle(DangTaiKeHoachLcntLenMangDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await DangTaiKeHoachLcntLenMang.GetOrderedSet()
            // .Include(o => o.DanhSachToTrinh)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
