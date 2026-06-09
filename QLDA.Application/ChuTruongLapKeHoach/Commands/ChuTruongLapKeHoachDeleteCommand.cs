using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.ChuTruongLapKeHoachs.Commands;

public record ChuTruongLapKeHoachDeleteCommand(Guid Id) : IRequest<int> {
}

public record ChuTruongLapKeHoachDeleteCommandHandler : IRequestHandler<ChuTruongLapKeHoachDeleteCommand, int> {
    private readonly IRepository<ChuTruongLapKeHoach, Guid> ChuTruongLapKeHoach;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public ChuTruongLapKeHoachDeleteCommandHandler(IServiceProvider serviceProvider) {
        ChuTruongLapKeHoach = serviceProvider.GetRequiredService<IRepository<ChuTruongLapKeHoach, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = ChuTruongLapKeHoach.UnitOfWork;
    }

    public async Task<int> Handle(ChuTruongLapKeHoachDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await ChuTruongLapKeHoach.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);
        
        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}