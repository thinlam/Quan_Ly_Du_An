using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.QuyetDinhDuyetKHLCNTs.Commands;

public record QuyetDinhDuyetKHLCNTDeleteCommand(Guid Id) : IRequest<int> {
}

public record QuyetDinhDuyetKHLCNTDeleteCommandHandler : IRequestHandler<QuyetDinhDuyetKHLCNTDeleteCommand, int> {
    private readonly IRepository<QuyetDinhDuyetKHLCNT, Guid> QuyetDinhDuyetKHLCNT;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDuyetKHLCNTDeleteCommandHandler(IServiceProvider serviceProvider) {
        QuyetDinhDuyetKHLCNT = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetKHLCNT, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _unitOfWork = QuyetDinhDuyetKHLCNT.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDuyetKHLCNTDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await QuyetDinhDuyetKHLCNT.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);
        
        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}