using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.GoiThaus.Commands;

public record GoiThauDeleteCommand(Guid Id) : IRequest {
}

public record GoiThauDeleteCommandHandler : IRequestHandler<GoiThauDeleteCommand> {
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<KetQuaTrungThau, Guid> _KetQuaTrungThau;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;
    
    public GoiThauDeleteCommandHandler(IServiceProvider serviceProvider) {
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        _KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _unitOfWork = GoiThau.UnitOfWork;
    }

    public async Task Handle(GoiThauDeleteCommand request, CancellationToken cancellationToken) {
        await ValidateAsync(request, cancellationToken);
        var entity = await GoiThau.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        var hasKetQua = await _KetQuaTrungThau.GetQueryableSet().AnyAsync(e => e.Id == entity.Id  && !e.IsDeleted, cancellationToken);
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
