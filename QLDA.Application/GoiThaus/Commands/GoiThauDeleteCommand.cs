using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.GoiThaus.Commands;

public record GoiThauDeleteCommand(Guid Id) : IRequest {
}

public record GoiThauDeleteCommandHandler : IRequestHandler<GoiThauDeleteCommand> {
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<KetQuaTrungThau, Guid> _KetQuaTrungThau;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    private readonly IUnitOfWork _unitOfWork;
    
    public GoiThauDeleteCommandHandler(IServiceProvider serviceProvider) {
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = GoiThau.UnitOfWork;
    }

    public async Task Handle(GoiThauDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await GoiThau.GetOrderedSet().Include(o => o.DuAn)
                            .FirstOrDefaultAsync( o => o.Id == request.Id,cancellationToken);
        ManagedException.ThrowIfNull(entity);

        var canExecute = await _authManager.CanExecuteAsync(AuthorizationResourceKeys.DuAn, entity.DuAn ?? new DuAn(), cancellationToken);
        if (!canExecute) throw new ForbiddenException("Bạn không có quyền!");

        await ValidateAsync(request, cancellationToken);

        var hasKetQua = await _KetQuaTrungThau.GetQueryableSet().AnyAsync(e => e.Id == entity.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull("Đã có kết quả lcnt! Không thể xóa.");

        await RemoveAsync(entity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    #region  Private helper methods

    private async Task ValidateAsync(GoiThauDeleteCommand request, CancellationToken cancellationToken) {
       
        var hasHopDong = await GoiThau.GetQueryableSet().AnyAsync(e => e.Id == request.Id && e.HopDong != null && !e.HopDong.IsDeleted, cancellationToken);
        var hasKetQuaTrungThau = await GoiThau.GetQueryableSet().AnyAsync(e => e.Id == request.Id && e.KetQuaTrungThau != null && !e.KetQuaTrungThau.IsDeleted, cancellationToken);

        ManagedException.ThrowIf(
            when: hasHopDong,
            message: "Gói thầu đã có hợp đồng không thể xoá!"
        );
        ManagedException.ThrowIf(
            when: hasKetQuaTrungThau,
            message: "Gói thầu đã có kết quả trúng thầu không thể xoá!"
        );
    }

    private async Task RemoveAsync(GoiThau entity, CancellationToken cancellationToken) {

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);
    }

    #endregion
}
