using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.DeXuatChuTruongMois.Commands;

public record DeXuatChuTruongMoiDeleteCommand(Guid Id) : IRequest<int>
{
}

public record DeXuatChuTruongMoiDeleteCommandHandler : IRequestHandler<DeXuatChuTruongMoiDeleteCommand, int>
{
    private readonly IRepository<DeXuatChuTruongMoi, Guid> DeXuatChuTruongMoi;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatChuTruongMoiDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        DeXuatChuTruongMoi = serviceProvider.GetRequiredService<IRepository<DeXuatChuTruongMoi, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = DeXuatChuTruongMoi.UnitOfWork;
    }

    public async Task<int> Handle(DeXuatChuTruongMoiDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await DeXuatChuTruongMoi.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}