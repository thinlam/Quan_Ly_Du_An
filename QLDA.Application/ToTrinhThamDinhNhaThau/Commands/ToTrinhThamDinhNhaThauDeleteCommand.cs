using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Entities;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;

public record ToTrinhThamDinhNhaThauDeleteCommand(Guid Id) : IRequest<int>
{
}

public record ToTrinhThamDinhNhaThauDeleteCommandHandler : IRequestHandler<ToTrinhThamDinhNhaThauDeleteCommand, int>
{
    private readonly IRepository<Domain.Entities.ToTrinhThamDinhNhaThau, Guid> ToTrinhThamDinhNhaThau;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhThamDinhNhaThauDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        ToTrinhThamDinhNhaThau = serviceProvider.GetRequiredService<IRepository<Domain.Entities.ToTrinhThamDinhNhaThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = ToTrinhThamDinhNhaThau.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhThamDinhNhaThauDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await ToTrinhThamDinhNhaThau.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}