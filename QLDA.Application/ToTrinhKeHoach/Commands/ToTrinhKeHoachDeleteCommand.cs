using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.ToTrinhKeHoachs.Commands;

public record ToTrinhKeHoachDeleteCommand(Guid Id) : IRequest<int>
{
}

public record ToTrinhKeHoachDeleteCommandHandler : IRequestHandler<ToTrinhKeHoachDeleteCommand, int>
{
    private readonly IRepository<ToTrinhKeHoach, Guid> ToTrinhKeHoach;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhKeHoachDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        ToTrinhKeHoach = serviceProvider.GetRequiredService<IRepository<ToTrinhKeHoach, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = ToTrinhKeHoach.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhKeHoachDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await ToTrinhKeHoach.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}